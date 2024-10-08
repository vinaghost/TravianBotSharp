using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Notification.Handlers.MainWindowLoad
{
    public class ChromeUserAgentInstall : INotificationHandler<MainWindowLoaded>
    {
        private readonly IUseragentManager _useragentManager;
        private readonly IWaitingOverlayViewModel _waitingOverlayViewModel;

        public ChromeUserAgentInstall(IUseragentManager useragentManager, IWaitingOverlayViewModel waitingOverlayViewModel)
        {
            _useragentManager = useragentManager;
            _waitingOverlayViewModel = waitingOverlayViewModel;
        }

        public async Task Handle(MainWindowLoaded notification, CancellationToken cancellationToken)
        {
            await _waitingOverlayViewModel.ChangeMessage("loading chrome useragent");
            await Task.Run(_useragentManager.Load, cancellationToken);
        }
    }
}