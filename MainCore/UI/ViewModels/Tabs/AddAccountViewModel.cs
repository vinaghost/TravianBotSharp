using FluentValidation;
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
    public class AddAccountViewModel : TabViewModelBase
    {
        public AccountInput AccountInput { get; } = new();
        public AccessInput AccessInput { get; } = new();

        private readonly IValidator<AccessInput> _accessInputValidator;
        private readonly IValidator<AccountInput> _accountInputValidator;

        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;
        public ReactiveCommand<Unit, Unit> AddAccessCommand { get; }
        public ReactiveCommand<Unit, Unit> EditAccessCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteAccessCommand { get; }
        public ReactiveCommand<Unit, Unit> AddAccountCommand { get; }

        public AddAccountViewModel(IValidator<AccessInput> accessInputValidator, IValidator<AccountInput> accountInputValidator, IDialogService dialogService, IMediator mediator, WaitingOverlayViewModel waitingOverlayViewModel, IUnitOfRepository unitOfRepository)
        {
            _mediator = mediator;
            _waitingOverlayViewModel = waitingOverlayViewModel;

            _accessInputValidator = accessInputValidator;
            _accountInputValidator = accountInputValidator;
            _dialogService = dialogService;
            _unitOfRepository = unitOfRepository;

            AddAccessCommand = ReactiveCommand.Create(AddAccessCommandHandler);
            EditAccessCommand = ReactiveCommand.Create(EditAccessCommandHandler);
            DeleteAccessCommand = ReactiveCommand.Create(DeleteAccessCommandHandler);
            AddAccountCommand = ReactiveCommand.CreateFromTask(AddAccountCommandHandler);
            this.WhenAnyValue(vm => vm.SelectedAccess)
                .WhereNotNull()
                .Subscribe(x => x.CopyTo(AccessInput));
        }

        protected override async Task OnActive()
        {
            await Observable.Start(() =>
            {
                AccessInput.Clear();
                AccountInput.Clear();
            }, RxApp.MainThreadScheduler);
        }

        private void AddAccessCommandHandler()
        {
            var result = _accessInputValidator.Validate(AccessInput);

            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            AccountInput.Accesses.Add(AccessInput.Clone());
        }

        private void EditAccessCommandHandler()
        {
            var result = _accessInputValidator.Validate(AccessInput);

            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            AccessInput.CopyTo(SelectedAccess);
        }

        private void DeleteAccessCommandHandler()
        {
            AccountInput.Accesses.Remove(SelectedAccess);
            SelectedAccess = null;
        }

        private async Task AddAccountCommandHandler()
        {
            var results = _accountInputValidator.Validate(AccountInput);

            if (!results.IsValid)
            {
                _dialogService.ShowMessageBox("Error", results.ToString());
                return;
            }
            await _waitingOverlayViewModel.Show("adding account");

            var dto = AccountInput.ToDto();
            var success = await Task.Run(() => _unitOfRepository.AccountRepository.Add(dto));
            if (success) await _mediator.Publish(new AccountUpdated());

            await _waitingOverlayViewModel.Hide();
            _dialogService.ShowMessageBox("Information", success ? "Added account" : "Account is duplicated");
        }

        private AccessInput _selectedAccess;

        public AccessInput SelectedAccess
        {
            get => _selectedAccess;
            set => this.RaiseAndSetIfChanged(ref _selectedAccess, value);
        }
    }
}