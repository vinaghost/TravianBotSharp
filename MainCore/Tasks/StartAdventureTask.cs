using MainCore.Commands.Features.StartAdventure;
using MainCore.Commands.NextExecute;
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

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(AccountId, AccountSettingEnums.EnableAutoStartAdventure);
                if (!settingEnable) return false;

                return true;
            }
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            ToAdventurePageCommand.Handler toAdventurePageCommand,
            ExploreAdventureCommand.Handler exploreAdventureCommand,
            NextExecuteStartAdventureTaskCommand.Handler nextExecuteStartAdventureTaskCommand,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await toAdventurePageCommand.HandleAsync(new(), cancellationToken);
            if (result.IsFailed) return result;
            result = await exploreAdventureCommand.HandleAsync(new(), cancellationToken);
            if (result.IsFailed) return result;

            await nextExecuteStartAdventureTaskCommand.HandleAsync(new(task), cancellationToken);
            return Result.Ok();
        }
    }
}
