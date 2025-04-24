namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class AccountInfoUpdated
    {
        public sealed record Notification(AccountId AccountId) : ByAccountIdBase(AccountId), INotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            CancellationToken cancellationToken)
        {
            await ValueTask.CompletedTask;
        }
    }
}