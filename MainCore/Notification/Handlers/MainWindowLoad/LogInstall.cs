using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Notification.Handlers.MainWindowLoad
{
    public class LogInstall : INotificationHandler<MainWindowLoaded>
    {
        private readonly ILogService _logService;
        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;

        public LogInstall(ILogService logService, WaitingOverlayViewModel waitingOverlayViewModel)
        {
            _logService = logService;
            _waitingOverlayViewModel = waitingOverlayViewModel;
        }

        public async Task Handle(MainWindowLoaded notification, CancellationToken cancellationToken)
        {
            await _waitingOverlayViewModel.ChangeMessage("installing log system");
            await Task.Run(_logService.Load, cancellationToken);
        }
    }
}