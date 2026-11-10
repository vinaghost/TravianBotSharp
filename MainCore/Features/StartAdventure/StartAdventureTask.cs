namespace MainCore.Features.StartAdventure;

[Handler]
public sealed partial class StartAdventureTask(
    ChromeManager chromeManager,
    OpenAdventurePanelCommand.Handler openAdventurePanelCommand,
    ExploreAdeventureCommand.Handler exploreAdeventureCommand)
{
    public sealed class Task(AccountId accountId) : AccountTask(accountId)
    {
        protected override string TaskName => nameof(StartAdventureTask).Humanize();
    }

    private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
    {
        var browser = chromeManager.Get(task.AccountId);

        var canStartAdventure = await CanStartAdventure(browser.CurrentPage);
        if (!canStartAdventure) return Skip.Error.WithError("No adventure available");

        Result result;
        result = await openAdventurePanelCommand.HandleAsync(new(browser), cancellationToken);
        if (result.IsFailed) return result;

        result = await exploreAdeventureCommand.HandleAsync(new(browser), cancellationToken);
        if (result.IsFailed) return result;

        var adventureDuration = await browser.CurrentPage.GetAdventureDuration();
        task.ExecuteAt = DateTime.Now.Add(adventureDuration * 2);

        return Result.Ok();
    }

    private static async Task<bool> CanStartAdventure(IPage page)
    {
        var heroState = await page.GetHeroState();

        if (heroState.Health <= 30)
        {
            return false;
        }

        if (!heroState.IsAtHome)
        {
            return false;
        }

        if (!heroState.IsAdventureReady)
        {
            return false;
        }
        return true;
    }
}