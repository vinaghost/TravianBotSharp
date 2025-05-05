using MainCore.Notification.Base;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Notification.Handlers.MainWindowLoad
{
    [Handler]
    public static partial class ChromeUserAgentInstall
    {
        private static async ValueTask HandleAsync(
            INotification notification,
            IUseragentManager useragentManager, IWaitingOverlayViewModel waitingOverlayViewModel,
            CancellationToken cancellationToken)
        {
            await waitingOverlayViewModel.ChangeMessage("loading chrome useragent");
            await Task.Run(useragentManager.Load, cancellationToken);
        }
    }
}