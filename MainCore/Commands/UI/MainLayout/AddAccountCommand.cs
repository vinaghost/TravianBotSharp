using MainCore.Common.MediatR;
using MainCore.UI.Enums;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.UserControls;
using MediatR;

namespace MainCore.Commands.UI.MainLayout
{
    public class AddAccountCommand : ByListBoxItemBase, IRequest
    {
        public AddAccountCommand(ListBoxItemViewModel items) : base(items)
        {
        }
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
            var accounts = request.Items;
            accounts.SelectedItem = null;

            _accountTabStore.SetTabType(AccountTabType.AddAccount);
        }
    }
}