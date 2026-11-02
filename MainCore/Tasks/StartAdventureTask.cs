using MainCore.Commands.Features.StartAdventure;
using MainCore.Infrasturecture.Extensions;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class StartAdventureTask(
        ChromeManager chromeManager,
        CheckHeroReadyCommand.Handler checkHeroReadyCommand,
        OpenAdventurePanelCommand.Handler openAdventurePanelCommand,
        ExploreAdeventureCommand.Handler exploreAdeventureCommand)
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => "Start adventure";

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(AccountId, AccountSettingEnums.EnableAutoStartAdventure);
                if (!settingEnable) return false;

                return true;
            }
        }

        private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
        {
            var canStartAdventure = await checkHeroReadyCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (!canStartAdventure) return Skip.Error.WithError("No adventure available");

            Result result;
            result = await openAdventurePanelCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;

            result = await exploreAdeventureCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;

            var browser = chromeManager.Get(task.AccountId);
            var adventureDuration = await AdventureParser.GetAdventureDuration(browser.CurrentPage);
            task.ExecuteAt = DateTime.Now.Add(adventureDuration * 2);

            return Result.Ok();
        }
    }
}