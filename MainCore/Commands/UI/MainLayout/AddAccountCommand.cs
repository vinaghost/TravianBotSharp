using MainCore.UI.Enums;
using MainCore.UI.Stores;
using MediatR;

namespace MainCore.Commands.UI.MainLayout
{
    public class AddAccountCommand : IRequest
    {
    }

    public class AddAccountCommandHandler : IRequestHandler<AddAccountCommand>
    {
        private readonly AccountTabStore _accountTabStore;

        public AddAccountCommandHandler(AccountTabStore accountTabStore)
        {
            _accountTabStore = accountTabStore;
        }

        public async Task Handle(AddAccountCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            _accountTabStore.SetTabType(AccountTabType.AddAccount);
        }
    }
}