using Immediate.Handlers.Shared;

namespace MainCore.Notification.Handlers.MainWindowUnload
{
    [Handler]
    public static partial class UseragentClean
    {
        private static async ValueTask HandleAsync(
            MainWindowUnloaded notification,
            IUseragentManager useragentManager,
            CancellationToken cancellationToken)
        {
            await Task.Run(useragentManager.Dispose, cancellationToken);
        }
    }
}