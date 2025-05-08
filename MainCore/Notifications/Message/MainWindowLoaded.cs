using MainCore.Constraints;
using MainCore.Notifications.Handlers.MainWindowLoad;

namespace MainCore.Notifications.Message
{
    [Handler]
    public static partial class MainWindowLoaded
    {
        public sealed record Notification : INotification;

        private static async ValueTask HandleAsync(
            Notification notification,
            ChromeDriverInstall.Handler chromeDriverInstall,
            ChromeExtensionInstall.Handler chromeExtensionInstall,
            ChromeUserAgentInstall.Handler chromeUserAgentInstall,
            DatabaseInstall.Handler databaseInstall,
            CancellationToken cancellationToken)
        {
            await chromeDriverInstall.HandleAsync(notification, cancellationToken);
            await chromeExtensionInstall.HandleAsync(notification, cancellationToken);
            await chromeUserAgentInstall.HandleAsync(notification, cancellationToken);
            await databaseInstall.HandleAsync(notification, cancellationToken);
        }
    }
}