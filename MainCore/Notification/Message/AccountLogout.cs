using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class AccountLogout
    {
        public sealed record Notification(AccountId AccountId) : ByAccountIdBase(AccountId), INotification;

        public static async ValueTask HandleAsync(
            Notification notification,
            LoginTaskTrigger.Handler loginTaskTrigger,
            CancellationToken cancellationToken)
        {
            await loginTaskTrigger.HandleAsync(notification, cancellationToken);
        }
    }
}