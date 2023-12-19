using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.UI.DebugTab
{
    public class LoadEndpointAddress : ByAccountIdBase, IRequest<string>
    {
        public LoadEndpointAddress(AccountId accountId) : base(accountId)
        {
        }
    }

    public class LoadEndpointAddressHandler : IRequestHandler<LoadEndpointAddress, string>
    {
        private readonly IChromeManager _chromeManager;
        private readonly ITaskManager _taskManager;

        private const string NotOpen = "Chrome didn't open yet";

        public LoadEndpointAddressHandler(IChromeManager chromeManager, ITaskManager taskManager)
        {
            _chromeManager = chromeManager;
            _taskManager = taskManager;
        }

        public async Task<string> Handle(LoadEndpointAddress request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var status = _taskManager.GetStatus(request.AccountId);
            if (status == Common.Enums.StatusEnums.Offline) return NotOpen;
            var chromeBrowser = _chromeManager.Get(request.AccountId);
            var address = chromeBrowser.EndpointAddress;
            if (string.IsNullOrEmpty(address)) return NotOpen;
            return address;
        }
    }
}