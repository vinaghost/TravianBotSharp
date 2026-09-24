namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateQuestCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            ITaskManager taskManager
           )
        {
            await Task.CompletedTask;
            if (!(await QuestParser.IsQuestClaimable(browser.CurrentPage))) return;
            var (accountId, villageId) = command;
            var claimQuestTask = new ClaimQuestTask.Task(accountId, villageId);
            if (!claimQuestTask.CanStart(context) || taskManager.IsExist<ClaimQuestTask.Task>(accountId))
            {
                return;
            }
            taskManager.Add(claimQuestTask);
        }
    }
}