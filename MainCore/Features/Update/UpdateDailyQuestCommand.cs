using MainCore.Features.ClaimDailyQuest;

namespace MainCore.Features.Update;

[Handler]
public sealed partial class UpdateDailyQuestCommand(
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
        if (!(await browser.CurrentPage.IsDailyQuestClaimable())) return;
        if (taskManager.IsExist<ClaimDailyQuestTask.Task>(accountId)) return;

        using var context = contextFactory.CreateDbContext();
        if (!context.IsDailyQuestEnable(accountId)) return;

        taskManager.Add(new ClaimDailyQuestTask.Task(accountId));
    }
}