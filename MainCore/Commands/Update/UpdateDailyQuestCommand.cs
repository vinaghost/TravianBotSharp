namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateDailyQuestCommand(
         IChromeBrowser browser,
         IDbContextFactory<AppDbContext> contextFactory,
         TaskManager taskManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private async ValueTask HandleAsync(Command command)
        {
            if (!(await QuestParser.IsDailyQuestClaimable(browser.CurrentPage))) return;
            var accountId = command.AccountId;
            using var context = contextFactory.CreateDbContext();
            var claimQuestTask = new ClaimDailyQuestTask.Task(accountId);
            if (!claimQuestTask.CanStart(context) || taskManager.IsExist<ClaimDailyQuestTask.Task>(accountId))
            {
                return;
            }
            if (taskManager.IsExist<ClaimDailyQuestTask.Task>(accountId)) return;
            taskManager.Add(claimQuestTask);
        }
    }
}