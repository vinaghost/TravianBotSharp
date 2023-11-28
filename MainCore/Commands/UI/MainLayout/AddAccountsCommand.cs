using MainCore.UI.Enums;
using MainCore.UI.Stores;
using MediatR;

namespace MainCore.Commands.UI.MainLayout
{
    public class AddAccountsCommand : IRequest
    {
    }

    public class AddAccountsCommandHandler : IRequestHandler<AddAccountsCommand>
    {
        private readonly AccountTabStore _accountTabStore;

        public AddAccountsCommandHandler(AccountTabStore accountTabStore)
        {
            _accountTabStore = accountTabStore;
        }

        public async Task Handle(AddAccountsCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            _accountTabStore.SetTabType(AccountTabType.AddAccounts);
        }
    }
}