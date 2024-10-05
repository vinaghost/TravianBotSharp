using FluentValidation;
using MainCore.Commands.Abstract;
using MainCore.UI.Models.Input;

namespace MainCore.Commands.UI.Tabs
{
    [RegisterSingleton<SaveSettingCommand>]
    public class SaveSettingCommand :
        ICommand<(AccountId, AccountSettingInput)>,
        ICommand<(AccountId, FarmListSettingInput)>,
        ICommand<(AccountId, VillageId, VillageSettingInput)>,
        ICommand<(AccountId, VillageId, Dictionary<VillageSettingEnums, int>)>
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

        public async Task<Result> Execute((AccountId, FarmListSettingInput) data, CancellationToken cancellationToken)
        {
            var (accountId, farmListSettingInput) = data;
            var result = await _farmListSettingInputValidator.ValidateAsync(farmListSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return Result.Ok();
            }

            var settings = farmListSettingInput.Get();
            Execute(accountId, settings);

            await _mediator.Publish(new AccountSettingUpdated(accountId));
            _dialogService.ShowMessageBox("Information", message: "Settings saved");
            return Result.Ok();
        }

        public async Task<Result> Execute((AccountId, VillageId, VillageSettingInput) data, CancellationToken cancellationToken)
        {
            var (accountId, villageId, villageSettingInput) = data;
            var result = await _villageSettingInputValidator.ValidateAsync(villageSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return Result.Ok();
            }

            var settings = villageSettingInput.Get();
            Execute(villageId, settings);

            await _mediator.Publish(new VillageSettingUpdated(accountId, villageId));
            _dialogService.ShowMessageBox("Information", "Settings saved");
            return Result.Ok();
        }

        public async Task<Result> Execute((AccountId, VillageId, Dictionary<VillageSettingEnums, int>) data, CancellationToken cancellationToken)
        {
            var (accountId, villageId, settings) = data;
            Execute(villageId, settings);

            await _mediator.Publish(new VillageSettingUpdated(accountId, villageId));
            _dialogService.ShowMessageBox("Information", "Settings saved");
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