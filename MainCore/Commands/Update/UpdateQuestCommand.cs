namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateQuestCommand(
        IChromeBrowser browser,
        IDbContextFactory<AppDbContext> contextFactory,
        TaskManager taskManager)
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private async ValueTask HandleAsync(Command command)
        {
            await Task.CompletedTask;
            if (!(await QuestParser.IsQuestClaimable(browser.CurrentPage))) return;
            var (accountId, villageId) = command;
            using var context = contextFactory.CreateDbContext();
            var claimQuestTask = new ClaimVillageQuestTask.Task(accountId, villageId);
            if (!claimQuestTask.CanStart(context) || taskManager.IsExist<ClaimVillageQuestTask.Task>(accountId))
            {
                return;
            }
            if (taskManager.IsExist<ClaimVillageQuestTask.Task>(accountId, villageId)) return;
            taskManager.Add(claimQuestTask);
        }
    }
}