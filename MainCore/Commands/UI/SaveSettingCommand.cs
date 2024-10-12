using FluentValidation;
using MainCore.UI.Models.Input;

namespace MainCore.Commands.UI
{
    [RegisterSingleton<SaveSettingCommand>]
    public class SaveSettingCommand
    {
        private readonly IValidator<VillageSettingInput> _villageSettingInputValidator;
        private readonly IValidator<AccountSettingInput> _accountsettingInputValidator;
        private readonly IValidator<FarmListSettingInput> _farmListSettingInputValidator;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;

        public SaveSettingCommand(IValidator<AccountSettingInput> accountsettingInputValidator, IDialogService dialogService, IMediator mediator, IDbContextFactory<AppDbContext> contextFactory, IValidator<VillageSettingInput> villageSettingInputValidator, IValidator<FarmListSettingInput> farmListSettingInputValidator)
        {
            _accountsettingInputValidator = accountsettingInputValidator;
            _dialogService = dialogService;
            _mediator = mediator;
            _contextFactory = contextFactory;
            _villageSettingInputValidator = villageSettingInputValidator;
            _farmListSettingInputValidator = farmListSettingInputValidator;
        }

        public async Task Execute(AccountId accountId, AccountSettingInput accountSettingInput, CancellationToken cancellationToken)
        {
            var result = await _accountsettingInputValidator.ValidateAsync(accountSettingInput, cancellationToken);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            var settings = accountSettingInput.Get();
            Execute(accountId, settings);

            await _mediator.Publish(new AccountSettingUpdated(accountId), cancellationToken);
            _dialogService.ShowMessageBox("Information", message: "Settings saved");
        }

        public async Task Execute(AccountId accountId, FarmListSettingInput farmListSettingInput, CancellationToken cancellationToken)
        {
            var result = await _farmListSettingInputValidator.ValidateAsync(farmListSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            var settings = farmListSettingInput.Get();
            Execute(accountId, settings);

            await _mediator.Publish(new AccountSettingUpdated(accountId));
            _dialogService.ShowMessageBox("Information", "Settings saved");
        }

        public async Task Execute(AccountId accountId, VillageId villageId, VillageSettingInput villageSettingInput, CancellationToken cancellationToken)
        {
            var result = await _villageSettingInputValidator.ValidateAsync(villageSettingInput, cancellationToken);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            var settings = villageSettingInput.Get();
            Execute(villageId, settings);

            await _mediator.Publish(new VillageSettingUpdated(accountId, villageId), cancellationToken);
            _dialogService.ShowMessageBox("Information", "Settings saved");
        }

        public async Task Execute(AccountId accountId, VillageId villageId, Dictionary<VillageSettingEnums, int> settings, CancellationToken cancellationToken)
        {
            Execute(villageId, settings);

            await _mediator.Publish(new VillageSettingUpdated(accountId, villageId), cancellationToken);
            _dialogService.ShowMessageBox("Information", "Settings saved");
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

        private void Execute(VillageId villageId, Dictionary<VillageSettingEnums, int> settings)
        {
            if (settings.Count == 0) return;
            using var context = _contextFactory.CreateDbContext();

            foreach (var setting in settings)
            {
                context.VillagesSetting
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Setting == setting.Key)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Value, setting.Value));
            }
        }
    }
}