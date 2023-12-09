using FluentValidation;
using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.Models.Input;
using MediatR;
using System.Text.Json;

namespace MainCore.Commands.UI.AccountSetting
{
    public class ImportCommand : ByAccountIdBase, IRequest
    {
        public AccountSettingInput AccountSettingInput { get; }

        public ImportCommand(AccountId accountId, AccountSettingInput accountSettingInput) : base(accountId)
        {
            AccountSettingInput = accountSettingInput;
        }
    }

    public class ImportCommandHandler : IRequestHandler<ImportCommand>
    {
        private readonly IValidator<AccountSettingInput> _accountsettingInputValidator;
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;

        public ImportCommandHandler(IValidator<AccountSettingInput> accountsettingInputValidator, UnitOfRepository unitOfRepository, IDialogService dialogService, IMediator mediator)
        {
            _accountsettingInputValidator = accountsettingInputValidator;
            _unitOfRepository = unitOfRepository;
            _dialogService = dialogService;
            _mediator = mediator;
        }

        public async Task Handle(ImportCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var accountSettingInput = request.AccountSettingInput;

            var path = _dialogService.OpenFileDialog();
            Dictionary<AccountSettingEnums, int> settings;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path, cancellationToken);
                settings = JsonSerializer.Deserialize<Dictionary<AccountSettingEnums, int>>(jsonString);
            }
            catch
            {
                _dialogService.ShowMessageBox("Warning", "Invalid file.");
                return;
            }

            accountSettingInput.Set(settings);
            var result = _accountsettingInputValidator.Validate(accountSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            settings = accountSettingInput.Get();
            _unitOfRepository.AccountSettingRepository.Update(accountId, settings);
            await _mediator.Publish(new AccountSettingUpdated(accountId), cancellationToken);

            _dialogService.ShowMessageBox("Information", "Settings imported");
        }
    }
}