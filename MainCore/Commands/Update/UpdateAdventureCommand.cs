namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateAdventureCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountConstraint;

        private static async ValueTask HandleAsync(
           Command command,
           IChromeBrowser browser,
           ITaskManager taskManager,
           CancellationToken cancellationToken
           )
        {
            await Task.CompletedTask;
            if (!AdventureParser.CanStartAdventure(browser.Html)) return;
            taskManager.Add(new StartAdventureTask.Task(command.AccountId));
        }
    }
}