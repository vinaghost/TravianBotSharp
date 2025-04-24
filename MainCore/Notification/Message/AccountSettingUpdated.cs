using MainCore.Notification.Handlers.Refresh;
using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class AccountSettingUpdated
    {
        public sealed record Notification(AccountId AccountId) : ByAccountIdBase(AccountId), INotification;

        public static async ValueTask HandleAsync(
            Notification notification,
            StartAdventureTaskTrigger.Handler startAdventureTaskTrigger,
            AccountSettingRefresh.Handler accountSettingRefresh,
            CancellationToken cancellationToken)
        {
            await startAdventureTaskTrigger.HandleAsync(notification, cancellationToken);
            await accountSettingRefresh.HandleAsync(notification, cancellationToken);
        }
    }
}