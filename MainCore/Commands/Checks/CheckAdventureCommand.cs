using MainCore.Constraints;
using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Commands.Checks
{
    [Handler]
    public static partial class CheckAdventureCommand
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