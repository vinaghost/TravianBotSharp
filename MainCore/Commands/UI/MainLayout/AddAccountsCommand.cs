using MainCore.Common.MediatR;
using MainCore.UI.Enums;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Commands.UI.MainLayout
{
    public class AddAccountsCommand : ByListBoxItemBase, IRequest
    {
        public AddAccountsCommand(ListBoxItemViewModel items) : base(items)
        {
        }
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

            var accounts = request.Items;
            accounts.SelectedItem = null;
            _accountTabStore.SetTabType(AccountTabType.AddAccounts);
        }
    }
}