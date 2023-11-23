using MainCore.Notification.Message;
using MainCore.Services;
using MainCore.UI.ViewModels.UserControls;
using MediatR;

namespace MainCore.Notification.Handlers.MainWindowLoad
{
    public class ChromeExtensionInstall : INotificationHandler<MainWindowLoaded>
    {
        private readonly IChromeManager _chromeManager;
        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;

        public ChromeExtensionInstall(IChromeManager chromeManager, WaitingOverlayViewModel waitingOverlayViewModel)
        {
            _chromeManager = chromeManager;
            _waitingOverlayViewModel = waitingOverlayViewModel;
        }

        public async Task Handle(MainWindowLoaded notification, CancellationToken cancellationToken)
        {
            await _waitingOverlayViewModel.ChangeMessage("installing chrome extension");
            await Task.Run(_chromeManager.LoadExtension, cancellationToken);
        }
    }
}