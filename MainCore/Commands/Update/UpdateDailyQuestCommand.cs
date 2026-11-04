namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateDailyQuestCommand(
         IDbContextFactory<AppDbContext> contextFactory,
         TaskManager taskManager)
    {
        public sealed record Command(AccountId AccountId, IChromeBrowser Browser) : IAccountCommand
        {
            public string Name => "";
        }

        private async ValueTask HandleAsync(Command command)
        {
            var (accountId, browser) = command;
            if (!(await QuestParser.IsDailyQuestClaimable(browser.CurrentPage))) return;

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