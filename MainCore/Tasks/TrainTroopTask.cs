using MainCore.Commands.Features.TrainTroop;
using MainCore.Commands.UI.Misc;
using MainCore.Common.Errors.TrainTroop;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class TrainTroopTask
    {
        public sealed class Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId, string villageName) : base(accountId, villageId, villageName)
            {
            }

            protected override string TaskName => "Train troop";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            IDbContextFactory<AppDbContext> contextFactory,
            ITaskManager taskManager,
            TrainTroopCommand.Handler trainTroopCommand,
            SaveVillageSettingCommand.Handler saveVillageSettingCommand,
            CancellationToken cancellationToken)
        {
            Result result;
            using var context = contextFactory.CreateDbContext();

            var filterdSettings = context.VillagesSetting
                .Where(x => x.VillageId == task.VillageId.Value)
                .Where(x => TrainTroopCommand.BuildingSettings.Values.Contains(x.Setting))
                .Where(x => x.Value != 0)
                .Select(x => x.Setting)
                .ToList();

            var buildings = GetTrainTroopBuilding(filterdSettings);

            var settings = new Dictionary<VillageSettingEnums, int>();

            foreach (var building in buildings)
            {
                result = await trainTroopCommand.HandleAsync(new(task.AccountId, task.VillageId, building), cancellationToken);
                if (!result.IsFailed) continue;

                if (result.HasError<MissingBuilding>())
                {
                    settings.Add(TrainTroopCommand.BuildingSettings[building], 0);
                    continue;
                }

                if (result.HasError<MissingResource>())
                {
                    break;
                }
            }

            await saveVillageSettingCommand.HandleAsync(new(task.AccountId, task.VillageId, settings), cancellationToken);
            var seconds = context.ByName(task.VillageId, VillageSettingEnums.TrainTroopRepeatTimeMin, VillageSettingEnums.TrainTroopRepeatTimeMax, 60);
            task.ExecuteAt = DateTime.Now.AddSeconds(seconds);
            await taskManager.ReOrder(task.AccountId);
            return Result.Ok();
        }

        private static List<BuildingEnums> GetTrainTroopBuilding(List<VillageSettingEnums> settings)
        {
            var buildings = new List<BuildingEnums>();

            if (settings.Contains(VillageSettingEnums.BarrackTroop))
            {
                buildings.Add(BuildingEnums.Barracks);
            }
            if (settings.Contains(VillageSettingEnums.StableTroop))
            {
                buildings.Add(BuildingEnums.Stable);
            }
            if (settings.Contains(VillageSettingEnums.GreatBarrackTroop))
            {
                buildings.Add(BuildingEnums.GreatBarracks);
            }
            if (settings.Contains(VillageSettingEnums.GreatStableTroop))
            {
                buildings.Add(BuildingEnums.GreatStable);
            }
            if (settings.Contains(VillageSettingEnums.WorkshopTroop))
            {
                buildings.Add(BuildingEnums.Workshop);
            }
            return buildings;
        }
    }
}