namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateAdventureCommand(
        IChromeBrowser browser,
        IDbContextFactory<AppDbContext> contextFactory,
        ITaskManager taskManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountConstraint;

        private async ValueTask HandleAsync(Command command)
        {
            var canStartAdventure = await AdventureParser.CanStartAdventure(browser.CurrentPage);
            if (!canStartAdventure) return;

            var startAdventureTask = new StartAdventureTask.Task(command.AccountId);
            using var context = contextFactory.CreateDbContext();
            if (!startAdventureTask.CanStart(context) || taskManager.IsExist<StartAdventureTask.Task>(command.AccountId))
            {
                return;
            }

            if (taskManager.IsExist<StartAdventureTask.Task>(command.AccountId)) return;
            taskManager.Add(startAdventureTask);
        }
    }
}