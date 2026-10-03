namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateQuestCommand(
        IChromeBrowser browser,
        IDbContextFactory<AppDbContext> contextFactory,
        ITaskManager taskManager)
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private async ValueTask HandleAsync(Command command)
        {
            await Task.CompletedTask;
            if (!(await QuestParser.IsQuestClaimable(browser.CurrentPage))) return;
            var (accountId, villageId) = command;
            using var context = contextFactory.CreateDbContext();
            var claimQuestTask = new ClaimQuestTask.Task(accountId, villageId);
            if (!claimQuestTask.CanStart(context) || taskManager.IsExist<ClaimQuestTask.Task>(accountId))
            {
                return;
            }
            taskManager.Add(claimQuestTask);
        }
    }
}