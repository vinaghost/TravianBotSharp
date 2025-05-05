using MainCore.Notification.Base;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class AccountInfoUpdated
    {
        public sealed record Notification(AccountId AccountId) : IAccountNotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            CancellationToken cancellationToken)
        {
            await ValueTask.CompletedTask;
        }
    }
}