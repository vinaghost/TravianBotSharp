using MainCore.Commands.UI.AccountSetting;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class AccountSettingViewModel : AccountTabViewModelBase
    {
        public AccountSettingInput AccountSettingInput { get; } = new();

        private readonly UnitOfRepository _unitOfRepository;
        private readonly IMediator _mediator;
        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> Export { get; }
        public ReactiveCommand<Unit, Unit> Import { get; }

        public ReactiveCommand<AccountId, Dictionary<AccountSettingEnums, int>> LoadSettings { get; }

        public AccountSettingViewModel(UnitOfRepository unitOfRepository, IMediator mediator)
        {
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;

            Save = ReactiveCommand.CreateFromTask(SaveHandler);
            Export = ReactiveCommand.CreateFromTask(ExportHandler);
            Import = ReactiveCommand.CreateFromTask(ImportHandler);
            LoadSettings = ReactiveCommand.Create<AccountId, Dictionary<AccountSettingEnums, int>>(LoadSettingsHandler);

            LoadSettings.Subscribe(x => AccountSettingInput.Set(x));
        }

        public async Task SettingRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadSettings.Execute(accountId).SubscribeOn(RxApp.TaskpoolScheduler);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadSettings.Execute(accountId).SubscribeOn(RxApp.TaskpoolScheduler);
        }

        private async Task SaveHandler()
        {
            await _mediator.Send(new SaveCommand(AccountId, AccountSettingInput));
        }

        private async Task ImportHandler()
        {
            await _mediator.Send(new ImportCommand(AccountId, AccountSettingInput));
        }

        private async Task ExportHandler()
        {
            await _mediator.Send(new ExportCommand(AccountId));
        }

        private Dictionary<AccountSettingEnums, int> LoadSettingsHandler(AccountId accountId)
        {
            var settings = _unitOfRepository.AccountSettingRepository.Get(accountId);
            return settings;
        }
    }
}