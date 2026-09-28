using Polly;

namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateVillageListCommand(
        IChromeBrowser browser,
        IDbContextFactory<AppDbContext> contextFactory,
        IRxQueue rxQueue,
        ITaskManager taskManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private async ValueTask HandleAsync(
            Command command)
        {
            await Task.CompletedTask;
            var accountId = command.AccountId;

            var dtos = await VillagePanelParser.Get(browser.CurrentPage);
            if (dtos.Count == 0) return;

            UpdateToDatabase(accountId, dtos);
            rxQueue.Enqueue(new VillagesModified(accountId));
            TriggerUpdateBuildingTask(accountId);
        }

        private void TriggerUpdateBuildingTask(AccountId accountId)
        {
            using var context = contextFactory.CreateDbContext();
            var settingEnable = context.BooleanByName(accountId, AccountSettingEnums.EnableAutoLoadVillageBuilding);
            if (!settingEnable) return;

            var villages = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.Buildings.Count < 40)
                .Select(x => new VillageId(x.Id))
                .ToList();

            foreach (var village in villages)
            {
                if (taskManager.IsExist<UpdateBuildingTask.Task>(accountId, village)) continue;
                taskManager.AddOrUpdate<UpdateBuildingTask.Task>(new(accountId, village));
            }
        }

        private void UpdateToDatabase(AccountId accountId, List<VillageDto> dtos)
        {
            using var context = contextFactory.CreateDbContext();
            var villages = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .ToList();

            var ids = dtos.Select(x => x.Id.Value).ToList();

            var villageDeleted = villages.Where(x => !ids.Contains(x.Id)).ToList();
            var villageInserted = dtos.Where(x => !villages.Exists(v => v.Id == x.Id.Value)).ToList();
            var villageUpdated = villages.Where(x => ids.Contains(x.Id)).ToList();

            villageDeleted.ForEach(x => context.Remove(x));
            villageInserted.ForEach(x =>
            {
                context.Add(x.ToEntity(accountId));
                context.FillVillageSettings(accountId, x.Id);
            });

            foreach (var village in villageUpdated)
            {
                var dto = dtos.First(x => x.Id.Value == village.Id);
                dto.To(village);
                context.Update(village);
            }

            context.SaveChanges();
        }
    }
}