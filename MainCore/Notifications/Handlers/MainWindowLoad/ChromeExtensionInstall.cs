using MainCore.Constraints;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Notifications.Handlers.MainWindowLoad
{
    [Handler]
    public static partial class ChromeExtensionInstall
    {
        private static async ValueTask HandleAsync(
            INotification notification,
            IChromeManager chromeManager, IWaitingOverlayViewModel waitingOverlayViewModel,
            CancellationToken cancellationToken)
        {
            await waitingOverlayViewModel.ChangeMessage("installing chrome extension");
            await Task.Run(chromeManager.LoadExtension, cancellationToken);
        }
    }
}