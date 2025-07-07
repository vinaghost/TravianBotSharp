namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateQuestCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeBrowser browser,
            ITaskManager taskManager
           )
        {
            await Task.CompletedTask;
            if (!QuestParser.IsQuestClaimable(browser.Html)) return;
            var (accountId, villageId) = command;
            taskManager.Add(new ClaimQuestTask.Task(accountId, villageId));
        }
    }
}