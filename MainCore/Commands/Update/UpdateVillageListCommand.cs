using MainCore.Infrasturecture.Extensions;
using MainCore.UI.ViewModels.Tabs.Villages;
using System.Text.Json;

namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateVillageListCommand(
        IChromeBrowser browser,
        IDbContextFactory<AppDbContext> contextFactory,
        IRxQueue rxQueue,
        ITaskManager taskManager,
        IDefaultTemplatePathStore defaultTemplatePathStore)
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
                ApplyVillageSettingTemplate(context, x.Id);
                ApplyBuildingListTemplate(context, x.Id);
            });

            foreach (var village in villageUpdated)
            {
                var dto = dtos.First(x => x.Id.Value == village.Id);
                dto.To(village);
                context.Update(village);
            }

            context.SaveChanges();
        }

        private void ApplyVillageSettingTemplate(AppDbContext context, VillageId villageId)
        {
            var path = defaultTemplatePathStore.Get().VillageSettingsPath;
            if (string.IsNullOrWhiteSpace(path)) return;
            if (!File.Exists(path)) return;

            try
            {
                var jsonString = File.ReadAllText(path);
                var settings = JsonSerializer.Deserialize<Dictionary<VillageSettingEnums, int>>(jsonString);
                if (settings is null) return;
                if (settings.Count == 0) return;

                settings.Remove(VillageSettingEnums.Tribe);
                context.SaveVillageSetting(villageId, settings);
            }
            catch
            {
                return;
            }
        }

        private void ApplyBuildingListTemplate(AppDbContext context, VillageId villageId)
        {
            var path = defaultTemplatePathStore.Get().BuildingListPath;
            if (string.IsNullOrWhiteSpace(path)) return;
            if (!File.Exists(path)) return;

            List<JobDto> jobs;
            try
            {
                var jsonString = File.ReadAllText(path);
                jobs = JsonSerializer.Deserialize<List<JobDto>>(jsonString) ?? [];
            }
            catch
            {
                return;
            }

            if (jobs.Count == 0) return;

            try
            {
                var fixedJobs = context.FixJobs(villageId, jobs, shuffle: true);
                var count = context.Jobs
                    .Count(x => x.VillageId == villageId.Value);

                var additionJobs = fixedJobs
                    .Select((job, index) => new Job
                    {
                        Position = count + index,
                        VillageId = villageId.Value,
                        Type = job.Type,
                        Content = job.Content,
                    })
                    .ToList();

                if (additionJobs.Count == 0) return;
                context.AddRange(additionJobs);
            }
            catch
            {
                return;
            }
        }
    }
}