namespace MainCore.Notification.Handlers.MainWindowUnload
{
    [Handler]
    public static partial class ProxyCacheClean
    {
        private static async ValueTask HandleAsync(
            INotification notification,
            CancellationToken cancellationToken)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Plugins");
            if (Directory.Exists(path))
            {
                await Task.Run(() => Directory.Delete(path, true), cancellationToken);
            }
        }
    }
}