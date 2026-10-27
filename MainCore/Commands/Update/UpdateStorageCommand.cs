namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateStorageCommand(
        IDbContextFactory<AppDbContext> contextFactory,
        IChromeBrowser browser,
        TaskManager taskManager,
        RxQueue rxQueue)
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private async ValueTask HandleAsync(
            Command command)
        {
            var (accountId, villageId) = command;

            var dto = await StorageParser.GetStorage(browser.CurrentPage);
            UpdateStorage(villageId, dto);
            rxQueue.Enqueue(new MainCore.Services.Telegram.TelegramVillageScan(accountId, villageId,
                Storage: new(dto.Wood, dto.Clay, dto.Iron, dto.Crop, dto.Warehouse, dto.Granary)));
            TriggerNpcTask(accountId, villageId);
        }

        private void TriggerNpcTask(AccountId accountId, VillageId villageId)
        {
            using var context = contextFactory.CreateDbContext();
            var task = new NpcTask.Task(accountId, villageId);
            if (task.CanStart(context) && !taskManager.IsExist<NpcTask.Task>(accountId, villageId))
            {
                taskManager.Add(task);
            }
        }

        private void UpdateStorage(VillageId villageId, StorageDto dto)
        {
            using var context = contextFactory.CreateDbContext();
            var dbStorage = context.Storages
                .FirstOrDefault(x => x.VillageId == villageId.Value);

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
