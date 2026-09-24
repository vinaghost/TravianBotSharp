namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateAdventureCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountConstraint;

        private static async ValueTask HandleAsync(
           Command command,
           IChromeBrowser browser,
           AppDbContext context,
           ITaskManager taskManager
           )
        {
            if (!(await AdventureParser.CanStartAdventure(browser.CurrentPage))) return;
            var startAdventureTask = new StartAdventureTask.Task(command.AccountId);
            if (!startAdventureTask.CanStart(context) || taskManager.IsExist<StartAdventureTask.Task>(command.AccountId))
            {
                return;
            }

            taskManager.Add(startAdventureTask);
        }
    }
}