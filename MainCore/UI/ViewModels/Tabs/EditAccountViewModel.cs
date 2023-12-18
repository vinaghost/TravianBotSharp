using MainCore.Commands.UI.Account;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using MediatR;
using ReactiveUI;
using System.Reactive.Linq;
using Unit = System.Reactive.Unit;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class EditAccountViewModel : AccountTabViewModelBase
    {
        public AccountInput AccountInput { get; } = new();
        public AccessInput AccessInput { get; } = new();

        private readonly IMediator _mediator;
        private readonly UnitOfRepository _unitOfRepository;
        public ReactiveCommand<Unit, Unit> AddAccess { get; }
        public ReactiveCommand<Unit, Unit> EditAccess { get; }
        public ReactiveCommand<Unit, Unit> DeleteAccess { get; }
        public ReactiveCommand<Unit, Unit> EditAccount { get; }

        public ReactiveCommand<AccountId, AccountDto> LoadAccount { get; }

        public EditAccountViewModel(IMediator mediator, UnitOfRepository unitOfRepository)
        {
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;

            AddAccess = ReactiveCommand.CreateFromTask(AddAccessHandler);
            EditAccess = ReactiveCommand.CreateFromTask(EditAccessHandler);
            DeleteAccess = ReactiveCommand.CreateFromTask(DeleteAccessHandler);
            EditAccount = ReactiveCommand.CreateFromTask(EditAccountHandler);
            LoadAccount = ReactiveCommand.Create<AccountId, AccountDto>(LoadAccountHandler);

            this.WhenAnyValue(vm => vm.SelectedAccess)
                .WhereNotNull()
                .Subscribe(x => x.CopyTo(AccessInput));

            DeleteAccess.Subscribe(x => SelectedAccess = null);
            LoadAccount.Subscribe(SetAccount);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadAccount.Execute().SubscribeOn(RxApp.TaskpoolScheduler);
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

        private async Task EditAccountHandler()
        {
            await _mediator.Send(new EditAccountCommand(AccountInput));
            await LoadAccount.Execute().SubscribeOn(RxApp.TaskpoolScheduler);
        }

        private AccountDto LoadAccountHandler(AccountId accountId)
        {
            var account = _unitOfRepository.AccountRepository.Get(AccountId, true);
            return account;
        }

        private void SetAccount(AccountDto account)
        {
            AccountInput.Id = account.Id;
            AccountInput.Username = account.Username;
            AccountInput.Server = account.Server;
            AccountInput.SetAccesses(account.Accesses.Select(x => x.ToInput()));

            AccessInput.Clear();
        }

        private AccessInput _selectedAccess;

        public AccessInput SelectedAccess
        {
            get => _selectedAccess;
            set => this.RaiseAndSetIfChanged(ref _selectedAccess, value);
        }
    }
}