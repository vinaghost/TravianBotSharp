using MainCore.Commands.Features.TrainTroop;
using MainCore.Commands.NextExecute;
using MainCore.Commands.UI.Misc;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class TrainTroopTask
    {
        public sealed class Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId) : base(accountId, villageId)
            {
            }

            protected override string TaskName => "Train troop";

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(VillageId, VillageSettingEnums.TrainTroopEnable);
                if (!settingEnable) return false;
                return true;
            }
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            GetTrainTroopBuildingCommand.Handler getTrainTroopBuildingQuery,
            ToTrainTroopPageCommand.Handler toTrainTroopPageCommand,
            TrainTroopCommand.Handler trainTroopCommand,
            SaveVillageSettingCommand.Handler saveVillageSettingCommand,
            NextExecuteTrainTroopTaskCommand.Handler nextExecuteTrainTroopTaskCommand,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            Result result;
            var buildings = await getTrainTroopBuildingQuery.HandleAsync(new(task.VillageId), cancellationToken);
            var settings = new Dictionary<VillageSettingEnums, int>();

            foreach (var building in buildings)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;

                result = await toTrainTroopPageCommand.HandleAsync(new(task.VillageId, building), cancellationToken);
                if (result.IsFailed)
                {
                    if (result.HasError<MissingBuilding>())
                    {
                        logger.Warning("Disable train troop on this building.", building);
                        settings.Add(TrainTroopCommand.TroopSettings[building], 0);
                        continue;
                    }
                    return result;
                }

                result = await trainTroopCommand.HandleAsync(new(task.VillageId, building), cancellationToken);
                if (result.IsFailed)
                {
                    if (result.HasError<MissingResource>())
                    {
                        break;
                    }
                    return result;
                }
            }

            await saveVillageSettingCommand.HandleAsync(new(task.AccountId, task.VillageId, settings), cancellationToken);
            await nextExecuteTrainTroopTaskCommand.HandleAsync(task, cancellationToken);
            return Result.Ok();
        }
    }
}