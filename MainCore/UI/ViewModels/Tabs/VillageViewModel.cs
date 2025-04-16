using MainCore.Tasks;
using MainCore.UI.Enums;
using MainCore.UI.Models.Output;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<VillageViewModel>]
    public partial class VillageViewModel : AccountTabViewModelBase
    {
        private readonly VillageTabStore _villageTabStore;

        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        public ListBoxItemViewModel Villages { get; } = new();

        public VillageTabStore VillageTabStore => _villageTabStore;

        public VillageViewModel(VillageTabStore villageTabStore, ITaskManager taskManager, IDialogService dialogService)
        {
            _villageTabStore = villageTabStore;
            _taskManager = taskManager;
            _dialogService = dialogService;

            var villageObservable = this.WhenAnyValue(x => x.Villages.SelectedItem);
            villageObservable.BindTo(_selectedItemStore, vm => vm.Village);
            villageObservable.Subscribe(x =>
            {
                var tabType = VillageTabType.Normal;
                if (x is null) tabType = VillageTabType.NoVillage;
                _villageTabStore.SetTabType(tabType);
            });

            LoadVillageCommand.Subscribe(Villages.Load);
        }

        public async Task VillageListRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadVillageCommand.Execute(accountId);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadVillageCommand.Execute(accountId);
        }

        [ReactiveCommand]
        private async Task LoadCurrent()
        {
            if (!Villages.IsSelected)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "No village selected"));
                return;
            }

            var villageId = new VillageId(Villages.SelectedItemId);
            await _taskManager.AddOrUpdate<UpdateBuildingTask>(AccountId, villageId);

            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Added update task"));
        }

        [ReactiveCommand]
        private async Task LoadUnload()
        {
            var getVillage = Locator.Current.GetService<GetVillage>();
            var villages = getVillage.Missing(AccountId);
            foreach (var village in villages)
            {
                await _taskManager.AddOrUpdate<UpdateBuildingTask>(AccountId, village);
            }
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Added update task"));
        }

        [ReactiveCommand]
        private async Task LoadAll()
        {
            var getVillage = Locator.Current.GetService<GetVillage>();
            var villages = getVillage.All(AccountId);
            foreach (var village in villages)
            {
                await _taskManager.AddOrUpdate<UpdateBuildingTask>(AccountId, village);
            }
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Added update task"));
        }

        [ReactiveCommand]
        private static List<ListBoxItem> LoadVillage(AccountId accountId)
        {
            var getVillage = Locator.Current.GetService<GetVillage>();
            return getVillage.Info(accountId);
        }
    }
}