using MainCore.Notification.Base;

namespace MainCore.Notification.Handlers.MainWindowUnload
{
    [Handler]
    public static partial class UseragentClean
    {
        private static async ValueTask HandleAsync(
            INotification notification,
            IUseragentManager useragentManager,
            CancellationToken cancellationToken)
        {
            await Task.Run(useragentManager.Dispose, cancellationToken);
        }
    }
}