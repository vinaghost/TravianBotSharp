using MainCore.Constraints;

namespace MainCore.Notification.Message
{
    [Handler]
    public static partial class HeroItemUpdated
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