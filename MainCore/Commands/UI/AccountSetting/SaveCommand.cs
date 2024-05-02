using FluentValidation;
using MainCore.UI.Models.Input;

namespace MainCore.Commands.UI.AccountSetting
{
    public class SaveCommand : ByAccountIdBase, IRequest
    {
        public AccountSettingInput AccountSettingInput { get; }

        public SaveCommand(AccountId accountId, AccountSettingInput accountSettingInput) : base(accountId)
        {
            AccountSettingInput = accountSettingInput;
        }
    }

    public class SaveCommandHandler : IRequestHandler<SaveCommand>
    {
        private readonly IValidator<AccountSettingInput> _accountsettingInputValidator;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly IAccountSettingRepository _accountSettingRepository;

        public SaveCommandHandler(IValidator<AccountSettingInput> accountsettingInputValidator, IDialogService dialogService, IMediator mediator, IAccountSettingRepository accountSettingRepository)
        {
            _accountsettingInputValidator = accountsettingInputValidator;
            _dialogService = dialogService;
            _mediator = mediator;
            _accountSettingRepository = accountSettingRepository;
        }

        public async Task Handle(SaveCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var accountSettingInput = request.AccountSettingInput;
            var result = _accountsettingInputValidator.Validate(accountSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            var settings = accountSettingInput.Get();
            _accountSettingRepository.Update(accountId, settings);
            await _mediator.Publish(new AccountSettingUpdated(accountId), cancellationToken);
            _dialogService.ShowMessageBox("Information", message: "Settings saved");
        }
    }
}