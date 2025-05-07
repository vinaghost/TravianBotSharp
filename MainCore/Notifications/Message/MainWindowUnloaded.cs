using MainCore.Constraints;
using MainCore.Notifications.Handlers.MainWindowUnload;

namespace MainCore.Notifications.Message
{
    [Handler]
    public static partial class MainWindowUnloaded
    {
        public sealed record Notification : INotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            ChromeClean.Handler chromeClean,
            ProxyCacheClean.Handler proxyCacheClean,
            UseragentClean.Handler useragentClean,
            CancellationToken cancellationToken)
        {
            await chromeClean.HandleAsync(notification, cancellationToken);
            await proxyCacheClean.HandleAsync(notification, cancellationToken);
            await useragentClean.HandleAsync(notification, cancellationToken);
        }
    }
}