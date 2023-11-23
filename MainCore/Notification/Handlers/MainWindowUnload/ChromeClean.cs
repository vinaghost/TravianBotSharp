using MainCore.Notification.Message;
using MainCore.Services;
using MediatR;

namespace MainCore.Notification.Handlers.MainWindowUnload
{
    public class ChromeClean : INotificationHandler<MainWindowUnloaded>
    {
        private readonly IChromeManager _chromeManager;

        public ChromeClean(IChromeManager chromeManager)
        {
            _chromeManager = chromeManager;
        }

        public async Task Handle(MainWindowUnloaded notification, CancellationToken cancellationToken)
        {
            await Task.Run(_chromeManager.Shutdown, cancellationToken);
        }
    }
}