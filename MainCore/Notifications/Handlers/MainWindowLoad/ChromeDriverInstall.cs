using MainCore.Constraints;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Notifications.Handlers.MainWindowLoad
{
    [Handler]
    public static partial class ChromeDriverInstall
    {
        private static async ValueTask HandleAsync(
            INotification notification,
            IChromeDriverInstaller chromeDriverInstaller, IWaitingOverlayViewModel waitingOverlayViewModel,
            CancellationToken cancellationToken
        )
        {
            await waitingOverlayViewModel.ChangeMessage("installing chrome driver");
            await Task.Run(chromeDriverInstaller.Install, cancellationToken);
        }
    }
}