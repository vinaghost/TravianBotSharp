using FluentValidation;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;

namespace MainCore.Commands.UI
{
    [RegisterSingleton<SaveSettingCommand>]
    public class SaveSettingCommand
    {
        private readonly IValidator<VillageSettingInput> _villageSettingInputValidator;
        private readonly IValidator<AccountSettingInput> _accountsettingInputValidator;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IDialogService _dialogService;
        private readonly AccountSettingUpdated.Handler _accountSettingUpdated;
        private readonly VillageSettingUpdated.Handler _villageSettingUpdated;

        public SaveSettingCommand(IValidator<AccountSettingInput> accountsettingInputValidator, IDialogService dialogService, IDbContextFactory<AppDbContext> contextFactory, IValidator<VillageSettingInput> villageSettingInputValidator, AccountSettingUpdated.Handler accountSettingUpdated, VillageSettingUpdated.Handler villageSettingUpdated)
        {
            _accountsettingInputValidator = accountsettingInputValidator;
            _dialogService = dialogService;
            _contextFactory = contextFactory;
            _villageSettingInputValidator = villageSettingInputValidator;
            _accountSettingUpdated = accountSettingUpdated;
            _villageSettingUpdated = villageSettingUpdated;
        }

        public async Task Execute(AccountId accountId, AccountSettingInput accountSettingInput, CancellationToken cancellationToken)
        {
            var result = await _accountsettingInputValidator.ValidateAsync(accountSettingInput, cancellationToken);
            if (!result.IsValid)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
                return;
            }

            var settings = accountSettingInput.Get();
            Execute(accountId, settings);

            await _accountSettingUpdated.HandleAsync(new(accountId), cancellationToken);
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Settings saved"));
        }

        public async Task Execute(AccountId accountId, VillageId villageId, VillageSettingInput villageSettingInput, CancellationToken cancellationToken)
        {
            var result = await _villageSettingInputValidator.ValidateAsync(villageSettingInput, cancellationToken);
            if (!result.IsValid)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
                return;
            }

            var settings = villageSettingInput.Get();
            Execute(villageId, settings);

            await _villageSettingUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Settings saved"));
        }

        public async Task Execute(AccountId accountId, VillageId villageId, Dictionary<VillageSettingEnums, int> settings, CancellationToken cancellationToken)
        {
            Execute(villageId, settings);
            await _villageSettingUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
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