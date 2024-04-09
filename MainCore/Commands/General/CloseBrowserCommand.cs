using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.General
{
    public class CloseBrowserCommand : ByAccountIdBase, IRequest
    {
        public CloseBrowserCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class CloseBrowserCommandHandler : IRequestHandler<CloseBrowserCommand>
    {
        private readonly IChromeManager _chromeManager;

        public CloseBrowserCommandHandler(IChromeManager chromeManager)
        {
            _chromeManager = chromeManager;
        }

        public async Task Handle(CloseBrowserCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            await Task.Run(chromeBrowser.Close, CancellationToken.None);
        }
    }
}