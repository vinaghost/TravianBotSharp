using MainCore.Features.ClaimVillageQuest;

namespace MainCore.Features.Update;

[Handler]
public sealed partial class UpdateVillageQuestCommand(
    IDbContextFactory<AppDbContext> contextFactory,
    TaskManager taskManager)
{
    public sealed record Command(IChromeBrowser Browser, VillageId VillageId) : IVillageBrowserCommand
    {
        public string Name => "";
    }

    private async ValueTask HandleAsync(Command command)
    {
        var (browser, accountId, villageId) = command;
        if (!(await browser.CurrentPage.IsVillageQuestClaimable())) return;
        if (taskManager.IsExist<ClaimVillageQuestTask.Task>(accountId, villageId)) return;
        using var context = contextFactory.CreateDbContext();
        if (!context.IsVillageQuestEnable(villageId)) return;

        taskManager.Add(new ClaimVillageQuestTask.Task(accountId, villageId), true);
    }
}