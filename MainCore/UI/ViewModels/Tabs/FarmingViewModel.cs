using MainCore.Commands.UI.Farming;
using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using MediatR;
using ReactiveUI;
using System.Reactive.Linq;
using Unit = System.Reactive.Unit;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class FarmingViewModel : AccountTabViewModelBase
    {
        public FarmListSettingInput FarmListSettingInput { get; } = new();
        public ListBoxItemViewModel FarmLists { get; } = new();

        private readonly IMediator _mediator;
        private readonly IUnitOfRepository _unitOfRepository;

        public ReactiveCommand<Unit, Unit> UpdateFarmList { get; }
        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> ActiveFarmList { get; }
        public ReactiveCommand<Unit, Unit> Start { get; }
        public ReactiveCommand<Unit, Unit> Stop { get; }
        public ReactiveCommand<AccountId, List<ListBoxItem>> LoadFarmList { get; }
        public ReactiveCommand<AccountId, Dictionary<AccountSettingEnums, int>> LoadSetting { get; }

        public FarmingViewModel(IMediator mediator, IUnitOfRepository unitOfRepository)
        {
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;

            UpdateFarmList = ReactiveCommand.CreateFromTask(UpdateFarmListHandler);
            Start = ReactiveCommand.CreateFromTask(StartHandler);
            Stop = ReactiveCommand.CreateFromTask(StopHandler);

            Save = ReactiveCommand.CreateFromTask(SaveHandler);
            ActiveFarmList = ReactiveCommand.CreateFromTask(ActiveFarmListHandler);

            LoadFarmList = ReactiveCommand.CreateFromTask<AccountId, List<ListBoxItem>>(LoadFarmListHandler);
            LoadSetting = ReactiveCommand.CreateFromTask<AccountId, Dictionary<AccountSettingEnums, int>>(LoadSettingHandler);

            LoadFarmList.Subscribe(items => FarmLists.Load(items));
            LoadSetting.Subscribe(items => FarmListSettingInput.Set(items));
        }

        public async Task FarmListRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadFarmList.Execute(accountId);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadFarmList.Execute(accountId);
            await LoadSetting.Execute(accountId);
        }

        private async Task UpdateFarmListHandler()
        {
            await _mediator.Send(new UpdateFarmListCommand(AccountId));
        }

        private async Task StartHandler()
        {
            await _mediator.Send(new StartFarmListCommand(AccountId));
        }

        private async Task StopHandler()
        {
            await _mediator.Send(new StopFarmListCommand(AccountId));
        }

        private async Task SaveHandler()
        {
            await _mediator.Send(new SaveFarmListSettingsCommand(AccountId, FarmListSettingInput));
        }

        private async Task ActiveFarmListHandler()
        {
            await _mediator.Send(new ActiveFarmListCommand(AccountId, FarmLists.SelectedItem));
        }

        private async Task<Dictionary<AccountSettingEnums, int>> LoadSettingHandler(AccountId accountId)
        {
            var items = await Task.Run(() => _unitOfRepository.AccountSettingRepository.Get(accountId));
            return items;
        }

        private async Task<List<ListBoxItem>> LoadFarmListHandler(AccountId accountId)

        {
            var items = await Task.Run(() => _unitOfRepository.FarmRepository.GetItems(accountId));
            return items;
        }
    }
}