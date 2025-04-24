namespace MainCore.Notification.Handlers.MainWindowUnload
{
    [Handler]
    public static partial class ChromeClean
    {
        private static async ValueTask HandleAsync(
            INotification notification,
            IChromeManager chromeManager,
            CancellationToken cancellationToken)
        {
            await Task.Run(chromeManager.Shutdown, cancellationToken);
        }
    }
}