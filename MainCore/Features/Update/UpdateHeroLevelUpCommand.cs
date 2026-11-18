using MainCore.Features.SetHeroPoint;

namespace MainCore.Features.Update;

[Handler]
public sealed partial class UpdateHeroLevelUpCommand(
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

        if (taskManager.IsExist<SetHeroPointTask.Task>(accountId)) return;
        if (await browser.CurrentPage.Locator("#topBarHero i.levelUp.show").CountAsync() == 0) return;
        using var context = contextFactory.CreateDbContext();
        if (!context.IsSetHeroPointEnable(accountId)) return;
        taskManager.Add(new SetHeroPointTask.Task(accountId));
    }
}