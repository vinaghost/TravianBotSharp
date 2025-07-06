using MainCore.Commands.Features.CompleteImmediately;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class CompleteImmediatelyTask
    {
        public sealed class Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId) : base(accountId, villageId)
            {
            }

            protected override string TaskName => "Complete immediately";

            private static List<BuildingEnums> UnskippableBuildings = new()
            {
                BuildingEnums.Residence,
                BuildingEnums.Palace,
                BuildingEnums.CommandCenter,
            };

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(VillageId, VillageSettingEnums.CompleteImmediately);
                if (!settingEnable) return false;

                var queueBuildings = context.QueueBuildings
                    .Where(x => x.VillageId == VillageId.Value)
                    .ToList();

                // empty
                if (queueBuildings.Count == 0) return false;
                // contains unskippable building
                if (queueBuildings.Any(x => UnskippableBuildings.Contains(x.Type))) return false;

                var completeImmediatelyMinimumTime = context.ByName(VillageId, VillageSettingEnums.CompleteImmediatelyTime);
                var requiredTime = DateTime.Now.AddMinutes(completeImmediatelyMinimumTime);

                var firstQueueBuildingCompleteTime = queueBuildings
                    .OrderBy(x => x.CompleteTime)
                    .Select(x => x.CompleteTime)
                    .FirstOrDefault();

                if (requiredTime > firstQueueBuildingCompleteTime) return false;
                return true;
            }
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            ToDorfCommand.Handler toDorfCommand,
            CompleteImmediatelyCommand.Handler completeImmediatelyCommand,
            ITaskManager taskManager,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await toDorfCommand.HandleAsync(new(task.AccountId, 0), cancellationToken);
            if (result.IsFailed) return result;
            result = await completeImmediatelyCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
            if (result.IsFailed) return result;

            taskManager.Add(new UpgradeBuildingTask.Task(task.AccountId, task.VillageId));

            return Result.Ok();
        }
    }
}