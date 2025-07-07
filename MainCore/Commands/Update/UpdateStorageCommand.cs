namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateStorageCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private static async ValueTask<StorageDto> HandleAsync(
            Command command,
            AppDbContext context,
            IChromeBrowser browser,
            ITaskManager taskManager
            )
        {
            await Task.CompletedTask;
            var (accountId, villageId) = command;

            var html = browser.Html;
            var dto = Get(html);
            context.UpdateStorage(villageId, dto);

            var task = new NpcTask.Task(accountId, villageId);
            if (task.CanStart(context) && !taskManager.IsExist<NpcTask.Task>(accountId, villageId))
            {
                taskManager.Add(task);
            }
            return dto;
        }

        private static StorageDto Get(HtmlDocument doc)
        {
            var storage = new StorageDto()
            {
                Wood = StorageParser.GetWood(doc),
                Clay = StorageParser.GetClay(doc),
                Iron = StorageParser.GetIron(doc),
                Crop = StorageParser.GetCrop(doc),
                FreeCrop = StorageParser.GetFreeCrop(doc),
                Warehouse = StorageParser.GetWarehouseCapacity(doc),
                Granary = StorageParser.GetGranaryCapacity(doc)
            };
            return storage;
        }

        private static void UpdateStorage(this AppDbContext context, VillageId villageId, StorageDto dto)
        {
            var dbStorage = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault();

            if (dbStorage is null)
            {
                var storage = dto.ToEntity(villageId);
                context.Add(storage);
            }
            else
            {
                dto.To(dbStorage);
            }

            context.SaveChanges();
        }
    }
}