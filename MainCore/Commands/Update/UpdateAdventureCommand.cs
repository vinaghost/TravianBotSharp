using MainCore.Constraints;
using MainCore.Notifications.Handlers.Trigger;

namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateAdventureCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountConstraint;

        private static async ValueTask HandleAsync(
           Command command,
           IChromeBrowser browser,
           StartAdventureTaskTrigger.Handler startAdventureTaskTrigger,
           CancellationToken cancellationToken
           )
        {
            if (!AdventureParser.CanStartAdventure(browser.Html)) return;
            await startAdventureTaskTrigger.HandleAsync(command, cancellationToken);
        }
    }
}