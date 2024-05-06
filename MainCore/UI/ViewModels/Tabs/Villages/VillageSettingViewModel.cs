using MainCore.Commands.UI.VillageSetting;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterAsViewModel]
    public class VillageSettingViewModel : VillageTabViewModelBase
    {
        public VillageSettingInput VillageSettingInput { get; } = new();

        private readonly IMediator _mediator;

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportCommand { get; }
        public ReactiveCommand<Unit, Unit> ImportCommand { get; }
        public ReactiveCommand<VillageId, Dictionary<VillageSettingEnums, int>> LoadSetting { get; }

        public VillageSettingViewModel(IMediator mediator)
        {
            _mediator = mediator;

            SaveCommand = ReactiveCommand.CreateFromTask(SaveHandler);
            ExportCommand = ReactiveCommand.CreateFromTask(ExportHandler);
            ImportCommand = ReactiveCommand.CreateFromTask(ImportHandler);
            LoadSetting = ReactiveCommand.Create<VillageId, Dictionary<VillageSettingEnums, int>>(LoadSettingHandler);

            LoadSetting.Subscribe(settings => VillageSettingInput.Set(settings));
        }

        public async Task SettingRefresh(VillageId villageId)
        {
            if (!IsActive) return;
            if (villageId != VillageId) return;
            await LoadSetting.Execute(villageId).SubscribeOn(RxApp.TaskpoolScheduler);
        }

        protected override async Task Load(VillageId villageId)
        {
            await LoadSetting.Execute(villageId).SubscribeOn(RxApp.TaskpoolScheduler);
        }

        private async Task SaveHandler()
        {
            await _mediator.Send(new SaveCommand(AccountId, VillageId, VillageSettingInput));
        }

        private async Task ImportHandler()
        {
            await _mediator.Send(new ImportCommand(AccountId, VillageId, VillageSettingInput));
        }

        private async Task ExportHandler()
        {
            await _mediator.Send(new ExportCommand(VillageId));
        }

        private Dictionary<VillageSettingEnums, int> LoadSettingHandler(VillageId villageId)
        {
            var settings = new GetSetting().Get(villageId);
            return settings;
        }
    }
}