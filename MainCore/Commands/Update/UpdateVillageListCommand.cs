﻿namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateVillageListCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            IRxQueue rxQueue,
            ITaskManager taskManager)
        {
            await Task.CompletedTask;
            var accountId = command.AccountId;

            var dtos = VillagePanelParser.Get(browser.Html);
            if (!dtos.Any()) return;

            context.UpdateToDatabase(accountId, dtos.ToList());

            rxQueue.Enqueue(new VillagesModified(accountId));

            var settingEnable = context.BooleanByName(accountId, AccountSettingEnums.EnableAutoLoadVillageBuilding);
            if (!settingEnable) return;

            var missingBuildingVillagesSpec = new MissingBuildingVillagesSpec(accountId);

            var villages = context.Villages
                .WithSpecification(missingBuildingVillagesSpec)
                .ToList();

            foreach (var village in villages)
            {
                if (taskManager.IsExist<UpdateBuildingTask.Task>(accountId, village)) continue;
                taskManager.AddOrUpdate<UpdateBuildingTask.Task>(new(accountId, village));
            }
        }

        private static void UpdateToDatabase(this AppDbContext context, AccountId accountId, List<VillageDto> dtos)
        {
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