using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class CompleteImmediatelyTask(
        ToDorfCommand.Handler toDorfCommand,
        IChromeBrowser browser,
        ITaskManager taskManager)
    {
        public sealed class Task(AccountId accountId, VillageId villageId) : VillageTask(accountId, villageId)
        {
            protected override string TaskName => "Complete immediately";

            private static readonly List<BuildingEnums> UnskippableBuildings =
            [
                BuildingEnums.Residence,
                BuildingEnums.Palace,
                BuildingEnums.CommandCenter,
            ];

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

                var anyBuilding = queueBuildings
                    .Any(x => x.CompleteTime > requiredTime);

                return anyBuilding;
            }
        }

        private async ValueTask<Result> HandleAsync(
            Task task,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await toDorfCommand.HandleAsync(new(0), cancellationToken);
            if (result.IsFailed) return result;

            result = await InstantUpgrade();
            if (result.IsFailed) return result;

            taskManager.AddOrUpdate(new UpgradeBuildingTask.Task(task.AccountId, task.VillageId));

            return Result.Ok();
        }

        private async ValueTask<Result> InstantUpgrade()
        {
            var oldQueueCount = await BuildingLayoutParser.CountQueueBuilding(browser.CurrentPage);
            if (oldQueueCount == 0) return Result.Ok();

            var completeButton = CompleteImmediatelyParser.GetCompleteButton(browser.CurrentPage);
            var result = await browser.Click(completeButton);
            if (result.IsFailed) return result;

            var confirmButton = CompleteImmediatelyParser.GetConfirmButton(browser.CurrentPage);
            result = await browser.Click(confirmButton);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}