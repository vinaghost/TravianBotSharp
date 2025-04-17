using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Notification.Handlers.MainWindowLoad
{
    public class ChromeDriverInstall : INotificationHandler<MainWindowLoaded>, IEnableLogger
    {
        private readonly IChromeDriverInstaller _chromeDriverInstaller;
        private readonly IWaitingOverlayViewModel _waitingOverlayViewModel;

        public ChromeDriverInstall(IChromeDriverInstaller chromeDriverInstaller, IWaitingOverlayViewModel waitingOverlayViewModel)
        {
            _chromeDriverInstaller = chromeDriverInstaller;
            _waitingOverlayViewModel = waitingOverlayViewModel;
        }

        public async Task Handle(MainWindowLoaded notification, CancellationToken cancellationToken)
        {
            await _waitingOverlayViewModel.ChangeMessage("installing chrome driver");
            this.Log().Info("Installing chrome driver");
            await Task.Run(_chromeDriverInstaller.Install, cancellationToken);
            this.Log().Info("Chrome driver installed");
        }
    }
}