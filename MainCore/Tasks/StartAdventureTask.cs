using MainCore.Commands.Features.StartAdventure;
using MainCore.Commands.NextExecute;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class StartAdventureTask
    {
        public sealed record Task : AccountTask
        {
            public Task(AccountId accountId) : base(accountId)
            {
            }

            protected override string TaskName => "Start adventure";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            ToAdventurePageCommand.Handler toAdventurePageCommand,
            ExploreAdventureCommand.Handler exploreAdventureCommand,
            NextExecuteStartAdventureTaskCommand.Handler nextExecuteStartAdventureTaskCommand,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await toAdventurePageCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;
            result = await exploreAdventureCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;

            await nextExecuteStartAdventureTaskCommand.HandleAsync(task, cancellationToken);
            return Result.Ok();
        }
    }
}