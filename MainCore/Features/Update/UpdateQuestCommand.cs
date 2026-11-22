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
        if (taskManager.IsExist<ClaimVillageQuestTask.Task>(accountId, villageId)) return;
        if (await browser.CurrentPage.GetNewQuestSpeechBubble().CountAsync() == 0) return;
        using var context = contextFactory.CreateDbContext();
        if (!context.IsVillageQuestEnable(villageId)) return;

        taskManager.Add(new ClaimVillageQuestTask.Task(accountId, villageId), true);
    }
}