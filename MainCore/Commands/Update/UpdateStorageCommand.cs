namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateStorageCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            IChromeBrowser browser,
            ITaskManager taskManager
            )
        {
            var (accountId, villageId) = command;

            var dto = await Get(browser.CurrentPage);
            context.UpdateStorage(villageId, dto);

            var task = new NpcTask.Task(accountId, villageId);
            if (task.CanStart(context) && !taskManager.IsExist<NpcTask.Task>(accountId, villageId))
            {
                taskManager.Add(task);
            }
        }

        private static async Task<StorageDto> Get(IPage page)
        {
            var storage = new StorageDto()
            {
                Wood = await StorageParser.GetWood(page),
                Clay = await StorageParser.GetClay(page),
                Iron = await StorageParser.GetIron(page),
                Crop = await StorageParser.GetCrop(page),
                FreeCrop = await StorageParser.GetFreeCrop(page),
                Warehouse = await StorageParser.GetWarehouseCapacity(page),
                Granary = await StorageParser.GetGranaryCapacity(page)
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