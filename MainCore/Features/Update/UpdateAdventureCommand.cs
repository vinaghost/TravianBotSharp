using MainCore.Features.StartAdventure;

namespace MainCore.Features.Update;

[Handler]
public sealed partial class UpdateAdventureCommand(
    IDbContextFactory<AppDbContext> contextFactory,
    TaskManager taskManager)
{
    public sealed record Command(AccountId AccountId, IChromeBrowser Browser) : IAccountCommand
    {
        public string Name => "";
    }

    private async ValueTask HandleAsync(Command command)
    {
        var canStartAdventure = await CanStartAdventure(command.Browser.CurrentPage);
        if (!canStartAdventure) return;

        var startAdventureTask = new StartAdventureTask.Task(command.AccountId);
        using var context = contextFactory.CreateDbContext();
        if (!startAdventureTask.CanStart(context) || taskManager.IsExist<StartAdventureTask.Task>(command.AccountId))
        {
            return;
        }

        if (taskManager.IsExist<StartAdventureTask.Task>(command.AccountId)) return;
        taskManager.Add(startAdventureTask);
    }

    private static async Task<bool> CanStartAdventure(IPage page)
    {
        var heroState = await page.GetHeroState();

        if (!heroState.IsAdventureReady)
        {
            return false;
        }
        return true;
    }
}