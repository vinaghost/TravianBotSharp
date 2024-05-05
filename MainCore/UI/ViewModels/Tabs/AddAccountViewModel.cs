using MainCore.Commands.UI.Account;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class AddAccountViewModel : TabViewModelBase
    {
        public AccountInput AccountInput { get; } = new();
        public AccessInput AccessInput { get; } = new();

        private readonly IMediator _mediator;
        public ReactiveCommand<Unit, Unit> AddAccess { get; }
        public ReactiveCommand<Unit, Unit> EditAccess { get; }
        public ReactiveCommand<Unit, Unit> DeleteAccess { get; }
        public ReactiveCommand<Unit, Unit> AddAccount { get; }

        public AddAccountViewModel(IMediator mediator)
        {
            _mediator = mediator;

            AddAccess = ReactiveCommand.CreateFromTask(AddAccessHandler);
            EditAccess = ReactiveCommand.CreateFromTask(EditAccessHandler);
            DeleteAccess = ReactiveCommand.CreateFromTask(DeleteAccessHandler);
            AddAccount = ReactiveCommand.CreateFromTask(AddAccountHandler);

            this.WhenAnyValue(vm => vm.SelectedAccess)
                .WhereNotNull()
                .Subscribe(x => x.CopyTo(AccessInput));

            DeleteAccess.Subscribe(x => SelectedAccess = null);
            AddAccount.Subscribe(x =>
            {
                AccountInput.Clear();
                AccessInput.Clear();
            });
        }

        private async Task AddAccessHandler()
        {
            await _mediator.Send(new AddAccessCommand(AccessInput, AccountInput));
        }

        private async Task EditAccessHandler()
        {
            await _mediator.Send(new EditAccessCommand(AccessInput, SelectedAccess));
        }

        private async Task DeleteAccessHandler()
        {
            await _mediator.Send(new DeleteAccessCommand(SelectedAccess, AccountInput));
        }

        private async Task AddAccountHandler()
        {
            await _mediator.Send(new AddAccountCommand(AccountInput));
        }

        private AccessInput _selectedAccess;

        public AccessInput SelectedAccess
        {
            get => _selectedAccess;
            set => this.RaiseAndSetIfChanged(ref _selectedAccess, value);
        }
    }
}