using MainCore.Constraints;

namespace MainCore.Notifications.Handlers.MainWindowUnload
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