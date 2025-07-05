using MainCore.Commands.Features.TrainTroop;
using MainCore.Commands.NextExecute;
using MainCore.Commands.UI.Misc;
using MainCore.Errors.TrainTroop;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class TrainTroopTask
    {
        public sealed record Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId, string villageName) : base(accountId, villageId, villageName)
            {
            }

            protected override string TaskName => "Train troop";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            TrainTroopCommand.Handler trainTroopCommand,
            GetTrainTroopBuildingQuery.Handler getTrainTroopBuildingQuery,
            SaveVillageSettingCommand.Handler saveVillageSettingCommand,
            NextExecuteTrainTroopTaskCommand.Handler nextExecuteTrainTroopTaskCommand,
            CancellationToken cancellationToken)
        {
            Result result;
            var buildings = await getTrainTroopBuildingQuery.HandleAsync(new(task.VillageId), cancellationToken);

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
            await nextExecuteTrainTroopTaskCommand.HandleAsync(task, cancellationToken);
            return Result.Ok();
        }
    }
}