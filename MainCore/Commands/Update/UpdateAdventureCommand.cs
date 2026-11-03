using MainCore.Commands.Features.StartAdventure;

namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateAdventureCommand(
        CheckHeroReadyCommand.Handler checkHeroReadyCommand,
        IDbContextFactory<AppDbContext> contextFactory,
        TaskManager taskManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => "";
        }

        private async ValueTask HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var canStartAdventure = await checkHeroReadyCommand.HandleAsync(new(command.AccountId), cancellationToken);
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