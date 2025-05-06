using MainCore.Constraints;
using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class AdventureUpdated
    {
        public sealed record Notification(AccountId AccountId) : IAccountNotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            StartAdventureTaskTrigger.Handler startAdventureTaskTrigger,
            CancellationToken cancellationToken)
        {
            await startAdventureTaskTrigger.HandleAsync(notification, cancellationToken);
        }
    }
}