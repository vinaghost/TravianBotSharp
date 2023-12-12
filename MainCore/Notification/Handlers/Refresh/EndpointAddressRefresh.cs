using MainCore.Notification.Message;
using MainCore.UI.ViewModels.Tabs;
using MediatR;

namespace MainCore.Notification.Handlers.Refresh
{
    public class EndpointAddressRefresh : INotificationHandler<StatusUpdated>
    {
        private readonly DebugViewModel _debugViewModel;

        public EndpointAddressRefresh(DebugViewModel debugViewModel)
        {
            _debugViewModel = debugViewModel;
        }

        public async Task Handle(StatusUpdated notification, CancellationToken cancellationToken)
        {
            await _debugViewModel.EndpointAddressRefresh(notification.AccountId);
        }
    }
}