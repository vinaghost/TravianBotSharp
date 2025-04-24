using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.Logging;

namespace MainCore.Notification.Handlers.MainWindowLoad
{
    [Handler]
    public static partial class ChromeExtensionInstall
    {
        private static async ValueTask HandleAsync(
            MainWindowLoaded notification,
            IChromeManager chromeManager, IWaitingOverlayViewModel waitingOverlayViewModel, ILogger<MainWindowLoaded> logger,
            CancellationToken cancellationToken)
        {
            await waitingOverlayViewModel.ChangeMessage("installing chrome extension");
            logger.LogInformation("Installing chrome extension");
            await Task.Run(chromeManager.LoadExtension, cancellationToken);
            logger.LogInformation("Chrome extension installed");
        }
    }
}