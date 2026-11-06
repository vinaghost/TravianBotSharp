using MainCore.Commands.Features.StartAdventure;

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
            protected override string TaskName => nameof(StartAdventureTask).Humanize();

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(AccountId, AccountSettingEnums.EnableAutoStartAdventure);
                if (!settingEnable) return false;

                return true;
            }
        }

        private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
        {
            var browser = chromeManager.Get(task.AccountId);

            var canStartAdventure = await checkHeroReadyCommand.HandleAsync(new(browser), cancellationToken);
            if (!canStartAdventure) return Skip.Error.WithError("No adventure available");

            Result result;
            result = await openAdventurePanelCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;

            result = await exploreAdeventureCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;

            var adventureDuration = await AdventureParser.GetAdventureDuration(browser.CurrentPage);
            task.ExecuteAt = DateTime.Now.Add(adventureDuration * 2);

            return Result.Ok();
        }
    }
}