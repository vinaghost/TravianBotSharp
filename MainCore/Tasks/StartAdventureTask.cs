using MainCore.Commands.Features.StartAdventure;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class StartAdventureTask
    {
        public sealed class Task : AccountTask
        {
            public Task(AccountId accountId) : base(accountId)
            {
            }

            protected override string TaskName => "Start adventure";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            ITaskManager taskManager,
            IChromeBrowser browser,
            ToAdventurePageCommand.Handler toAdventurePageCommand,
            ExploreAdventureCommand.Handler exploreAdventureCommand,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await toAdventurePageCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;
            result = await exploreAdventureCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;

            var adventureDuration = AdventureParser.GetAdventureDuration(browser.Html);
            task.ExecuteAt = DateTime.Now.Add(adventureDuration * 2);
            taskManager.ReOrder(task.AccountId);
            return Result.Ok();
        }
    }
}