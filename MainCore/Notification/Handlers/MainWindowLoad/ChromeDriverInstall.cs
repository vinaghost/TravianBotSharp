using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.Logging;

namespace MainCore.Notification.Handlers.MainWindowLoad
{
    [Handler]
    public static partial class ChromeDriverInstall
    {
        private static async ValueTask HandleAsync(
            MainWindowLoaded notification,
            IChromeDriverInstaller chromeDriverInstaller, IWaitingOverlayViewModel waitingOverlayViewModel, ILogger<MainWindowLoaded> logger,
            CancellationToken cancellationToken
        )
        {
            await waitingOverlayViewModel.ChangeMessage("installing chrome driver");
            logger.LogInformation("Installing chrome driver");
            await Task.Run(chromeDriverInstaller.Install, cancellationToken);
            logger.LogInformation("Chrome driver installed");
        }
    }
}