using MainCore.Commands.Features.TrainTroop;
using MainCore.Commands.NextExecute;
using MainCore.Commands.UI.Misc;
using MainCore.Errors.TrainTroop;
using MainCore.Enums;
using MainCore.Services;
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
            TrainTroopCommand.Handler trainTroopCommand,
            GetTrainQueueTimeCommand.Handler getTrainQueueTimeCommand,
            GetTrainTroopBuildingQuery.Handler getTrainTroopBuildingQuery,
            SaveVillageSettingCommand.Handler saveVillageSettingCommand,
            NextExecuteTrainTroopTaskCommand.Handler nextExecuteTrainTroopTaskCommand,
            ISettingService settingService,
            CancellationToken cancellationToken)
        {
            Result result;
            var queueResult = await getTrainQueueTimeCommand.HandleAsync(new(task.AccountId, task.VillageId, BuildingEnums.Barracks), cancellationToken);
            if (queueResult.IsFailed) return queueResult.ToResult();
            var queueTime = queueResult.Value;

            var minQueue = TimeSpan.FromMinutes(settingService.ByName(task.VillageId, VillageSettingEnums.TrainTroopRepeatTimeMin));

            var buildings = await getTrainTroopBuildingQuery.HandleAsync(new(task.VillageId), cancellationToken);

            var settings = new Dictionary<VillageSettingEnums, int>();

            if (queueTime < minQueue)
            {
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

                queueTime = minQueue;
            }

            await saveVillageSettingCommand.HandleAsync(new(task.AccountId, task.VillageId, settings), cancellationToken);
            await nextExecuteTrainTroopTaskCommand.HandleAsync(new(task, queueTime), cancellationToken);
            return Result.Ok();
        }
    }
}