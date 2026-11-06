namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateQuestCommand(
        ChromeManager chromeManager,
        IDbContextFactory<AppDbContext> contextFactory,
        TaskManager taskManager)
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand
        {
            public string Name => "";
        }

        private async ValueTask HandleAsync(Command command)
        {
            var (accountId, villageId) = command;
            var browser = chromeManager.Get(accountId);
            if (!(await QuestParser.IsVillageQuestClaimable(browser.CurrentPage))) return;
            using var context = contextFactory.CreateDbContext();
            var claimQuestTask = new ClaimVillageQuestTask.Task(accountId, villageId);
            if (!claimQuestTask.CanStart(context) || taskManager.IsExist<ClaimVillageQuestTask.Task>(accountId))
            {
                return;
            }
            if (taskManager.IsExist<ClaimVillageQuestTask.Task>(accountId, villageId)) return;
            taskManager.Add(claimQuestTask, true);
        }
    }
}