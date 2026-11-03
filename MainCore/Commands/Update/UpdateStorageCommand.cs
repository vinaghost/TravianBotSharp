using System.Net;
using System.Text.Json;

namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateStorageCommand(
        IDbContextFactory<AppDbContext> contextFactory,
        ChromeManager chromeManager,
        TaskManager taskManager)
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand
        {
            public string Name => "";
        }

        private async ValueTask HandleAsync(Command command)
        {
            var (accountId, villageId) = command;
            var browser = chromeManager.Get(accountId);

            var dto = await GetData(browser.CurrentPage);
            UpdateStorage(villageId, dto);
            TriggerNpcTask(accountId, villageId);
        }

        private void TriggerNpcTask(AccountId accountId, VillageId villageId)
        {
            using var context = contextFactory.CreateDbContext();
            var task = new NPCTask.Task(accountId, villageId);
            if (task.CanStart(context) && !taskManager.IsExist<NPCTask.Task>(accountId, villageId))
            {
                taskManager.Add(task, true);
            }
        }

        private void UpdateStorage(VillageId villageId, StorageDto dto)
        {
            using var context = contextFactory.CreateDbContext();
            var dbStorage = context.Storages
                .FirstOrDefault(x => x.VillageId == villageId.Value);

            if (dbStorage is null)
            {
                var storage = new Storage
                {
                    VillageId = villageId.Value,
                    Wood = dto.Wood,
                    Clay = dto.Clay,
                    Iron = dto.Iron,
                    Crop = dto.Crop,
                    FreeCrop = dto.FreeCrop,
                    Warehouse = dto.Warehouse,
                    Granary = dto.Granary
                };
                context.Add(storage);
            }
            else
            {
                dbStorage.Wood = dto.Wood;
                dbStorage.Clay = dto.Clay;
                dbStorage.Iron = dto.Iron;
                dbStorage.Crop = dto.Crop;
                dbStorage.FreeCrop = dto.FreeCrop;
                dbStorage.Warehouse = dto.Warehouse;
                dbStorage.Granary = dto.Granary;
            }

            context.SaveChanges();
        }

        public record struct RawStorageDto(string? WoodText, string? ClayText, string? IronText, string? CropText, string? FreeCropText, string? WarehouseText, string? GranaryText);

        public static async Task<StorageDto> GetData(IPage page)
        {
            // 1. Instantly pull all raw text payloads out of the browser DOM at the exact same moment
            var jsonResult = await page.EvaluateAsync<JsonElement>(@"() => {
                const getRawText = (selector) => {
                    const el = document.querySelector(selector);
                    return el ? (el.innerText || el.textContent).trim() : '';
                };

                return {
                    WoodText: getRawText('#l1'),
                    ClayText: getRawText('#l2'),
                    IronText: getRawText('#l3'),
                    CropText: getRawText('#l4'),
                    FreeCropText: getRawText('#stockBarFreeCrop'),
                    WarehouseText: getRawText('#stockBar div.warehouse div.capacity div.value'),
                    GranaryText: getRawText('#stockBar div.granary div.capacity div.value')
                };
            }");

            var text = jsonResult.GetRawText();
            var raw = JsonSerializer.Deserialize<RawStorageDto>(text);

            static long ProcessValue(string? rawValue)
            {
                string decoded = WebUtility.HtmlDecode(rawValue ?? "");
                return decoded.ParseLong();
            }

            return new StorageDto
            {
                Wood = ProcessValue(raw.WoodText),
                Clay = ProcessValue(raw.ClayText),
                Iron = ProcessValue(raw.IronText),
                Crop = ProcessValue(raw.CropText),
                FreeCrop = ProcessValue(raw.FreeCropText),
                Warehouse = ProcessValue(raw.WarehouseText),
                Granary = ProcessValue(raw.GranaryText)
            };
        }
    }
}