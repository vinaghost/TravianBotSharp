using FluentValidation;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
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

        private readonly IValidator<AccessInput> _accessInputValidator;
        private readonly IValidator<AccountInput> _accountInputValidator;

        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;
        public ReactiveCommand<Unit, Unit> AddAccess { get; }
        public ReactiveCommand<Unit, Unit> EditAccess { get; }
        public ReactiveCommand<Unit, Unit> DeleteAccess { get; }
        public ReactiveCommand<Unit, Unit> EditAccount { get; }

        public EditAccountViewModel(WaitingOverlayViewModel waitingOverlayViewModel, IValidator<AccessInput> accessInputValidator, IValidator<AccountInput> accountInputValidator, IMediator mediator, IDialogService dialogService, IUnitOfRepository unitOfRepository)
        {
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _accessInputValidator = accessInputValidator;
            _accountInputValidator = accountInputValidator;

            _mediator = mediator;
            _dialogService = dialogService;
            _unitOfRepository = unitOfRepository;

            AddAccess = ReactiveCommand.Create(AddAccessHandler);
            EditAccess = ReactiveCommand.Create(EditAccessHandler);
            DeleteAccess = ReactiveCommand.Create(DeleteAccessHandler);
            EditAccount = ReactiveCommand.CreateFromTask(EditAccountHandler);

            this.WhenAnyValue(vm => vm.SelectedAccess)
                .WhereNotNull()
                .Subscribe(x => x.CopyTo(AccessInput));
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadAccount();
        }

        private void AddAccessHandler()
        {
            var result = _accessInputValidator.Validate(AccessInput);

            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            AccountInput.Accesses.Add(AccessInput.Clone());
        }

        private void EditAccessHandler()
        {
            var result = _accessInputValidator.Validate(AccessInput);

            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            AccessInput.CopyTo(SelectedAccess);
        }

        private void DeleteAccessHandler()
        {
            AccountInput.Accesses.Remove(SelectedAccess);
            SelectedAccess = null;
        }

        private async Task EditAccountHandler()
        {
            var results = _accountInputValidator.Validate(AccountInput);

            if (!results.IsValid)
            {
                _dialogService.ShowMessageBox("Error", results.ToString());
                return;
            }

            await _waitingOverlayViewModel.Show("editing account");

            var dto = AccountInput.ToDto();
            await Task.Run(() => _unitOfRepository.AccountRepository.Update(dto));
            await _mediator.Publish(new AccountUpdated());

            await _waitingOverlayViewModel.Hide();
            _dialogService.ShowMessageBox("Information", "Edited accounts");
        }

        private async Task LoadAccount()
        {
            var account = await Task.Run(() => _unitOfRepository.AccountRepository.Get(AccountId, true));

            await Observable.Start(() =>
            {
                AccountInput.Id = account.Id;
                AccountInput.Username = account.Username;
                AccountInput.Server = account.Server;
                AccountInput.SetAccesses(account.Accesses.Select(x => x.ToInput()));

                AccessInput.Clear();
            }, RxApp.MainThreadScheduler);
        }

        private AccessInput _selectedAccess;

        public AccessInput SelectedAccess
        {
            get => _selectedAccess;
            set => this.RaiseAndSetIfChanged(ref _selectedAccess, value);
        }
    }
}