using MainCore.Constraints;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Notifications.Handlers.MainWindowLoad
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