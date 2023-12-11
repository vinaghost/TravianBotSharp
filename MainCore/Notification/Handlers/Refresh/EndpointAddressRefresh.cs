using MainCore.Notification.Message;
using MainCore.UI.ViewModels.Tabs;
using MediatR;
using System.Reactive.Linq;

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
            await _debugViewModel.LoadEndpointAddress.Execute(notification.AccountId);
        }
    }
}