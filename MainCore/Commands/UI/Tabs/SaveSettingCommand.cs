using FluentValidation;
using MainCore.Commands.Abstract;
using MainCore.UI.Models.Input;

namespace MainCore.Commands.UI.Tabs
{
    [RegisterSingleton<SaveSettingCommand>]
    public class SaveSettingCommand : ICommand<(AccountId, AccountSettingInput)>
    {
        private readonly IValidator<AccountSettingInput> _accountsettingInputValidator;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;

        public SaveSettingCommand(IValidator<AccountSettingInput> accountsettingInputValidator, IDialogService dialogService, IMediator mediator, IDbContextFactory<AppDbContext> contextFactory)
        {
            _accountsettingInputValidator = accountsettingInputValidator;
            _dialogService = dialogService;
            _mediator = mediator;
            _contextFactory = contextFactory;
        }

        public async Task<Result> Execute((AccountId, AccountSettingInput) data, CancellationToken cancellationToken)
        {
            var (accountId, accountSettingInput) = data;
            var result = await _accountsettingInputValidator.ValidateAsync(accountSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return Result.Ok();
            }

            var settings = accountSettingInput.Get();
            Execute(accountId, settings);

            await _mediator.Publish(new AccountSettingUpdated(accountId));
            _dialogService.ShowMessageBox("Information", message: "Settings saved");
            return Result.Ok();
        }

        private void Execute(AccountId accountId, Dictionary<AccountSettingEnums, int> settings)
        {
            if (settings.Count == 0) return;
            using var context = _contextFactory.CreateDbContext();

            foreach (var setting in settings)
            {
                context.AccountsSetting
                    .Where(x => x.AccountId == accountId.Value)
                    .Where(x => x.Setting == setting.Key)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Value, setting.Value));
            }
        }
    }
}