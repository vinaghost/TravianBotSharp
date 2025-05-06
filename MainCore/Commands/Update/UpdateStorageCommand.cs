using MainCore.Constraints;

namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateStorageCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : ICommand;

        private static async ValueTask<StorageDto> HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            StorageUpdated.Handler storageUpdated,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId) = command;

            var html = browser.Html;

            var dto = Get(html);

            context.UpdateStorage(villageId, dto);
            await storageUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
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
                context.Update(dbStorage);
            }

            context.SaveChanges();
        }
    }
}