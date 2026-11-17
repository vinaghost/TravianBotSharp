using MainCore.Features.StartAdventure;

namespace MainCore.Features.Update;

[Handler]
public sealed partial class UpdateAdventureCommand(
    IDbContextFactory<AppDbContext> contextFactory,
    TaskManager taskManager)
{
    public sealed record Command(IChromeBrowser Browser) : IBrowserCommand
    {
        public string Name => "";
    }

    private async ValueTask HandleAsync(Command command)
    {
        var (browser, accountId) = command;

        var canStartAdventure = await CanStartAdventure(browser.CurrentPage);
        if (!canStartAdventure) return;

        if (taskManager.IsExist<StartAdventureTask.Task>(accountId)) return;

        using var context = contextFactory.CreateDbContext();
        if (!context.IsStartAdventureEnable(accountId)) return;

        taskManager.Add(new StartAdventureTask.Task(accountId));
    }

    private async Task<bool> CanStartAdventure(IPage page)
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