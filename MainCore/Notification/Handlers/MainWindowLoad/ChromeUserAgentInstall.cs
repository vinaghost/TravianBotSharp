using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.Logging;

namespace MainCore.Notification.Handlers.MainWindowLoad
{
    [Handler]
    public static partial class ChromeUserAgentInstall
    {
        private static async ValueTask HandleAsync(
            MainWindowLoaded notification,
            IUseragentManager useragentManager, IWaitingOverlayViewModel waitingOverlayViewModel, ILogger<MainWindowLoaded> logger,
            CancellationToken cancellationToken)
        {
            await waitingOverlayViewModel.ChangeMessage("loading chrome useragent");
            logger.LogInformation("Loading chrome useragent");
            await Task.Run(useragentManager.Load, cancellationToken);
            logger.LogInformation("Chrome useragent loaded");
        }
    }
}