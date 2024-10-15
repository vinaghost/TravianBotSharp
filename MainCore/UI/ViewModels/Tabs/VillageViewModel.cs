using MainCore.Tasks;
using MainCore.UI.Enums;
using MainCore.UI.Models.Output;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<VillageViewModel>]
    public class VillageViewModel : AccountTabViewModelBase
    {
        private readonly VillageTabStore _villageTabStore;

        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        public ListBoxItemViewModel Villages { get; } = new();

        public VillageTabStore VillageTabStore => _villageTabStore;
        public ReactiveCommand<Unit, Unit> LoadCurrent { get; }
        public ReactiveCommand<Unit, Unit> LoadUnload { get; }
        public ReactiveCommand<Unit, Unit> LoadAll { get; }
        public ReactiveCommand<AccountId, List<ListBoxItem>> LoadVillage { get; }

        public VillageViewModel(VillageTabStore villageTabStore, ITaskManager taskManager, IDialogService dialogService)
        {
            _villageTabStore = villageTabStore;
            _taskManager = taskManager;
            _dialogService = dialogService;

            LoadCurrent = ReactiveCommand.CreateFromTask(LoadCurrentHandler);
            LoadUnload = ReactiveCommand.CreateFromTask(LoadUnloadHandler);
            LoadAll = ReactiveCommand.CreateFromTask(LoadAllHandler);
            LoadVillage = ReactiveCommand.Create<AccountId, List<ListBoxItem>>(LoadVillageHandler);

            var villageObservable = this.WhenAnyValue(x => x.Villages.SelectedItem);
            villageObservable.BindTo(_selectedItemStore, vm => vm.Village);
            villageObservable.Subscribe(x =>
            {
                var tabType = VillageTabType.Normal;
                if (x is null) tabType = VillageTabType.NoVillage;
                _villageTabStore.SetTabType(tabType);
            });

            LoadVillage.Subscribe(Villages.Load);
        }

        public async Task VillageListRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadVillage.Execute(accountId);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadVillage.Execute(accountId);
        }

        private async Task LoadCurrentHandler()
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

        private async Task LoadUnloadHandler()
        {
            var getVillage = Locator.Current.GetService<GetVillage>();
            var villages = getVillage.Missing(AccountId);
            foreach (var village in villages)
            {
                await _taskManager.AddOrUpdate<UpdateBuildingTask>(AccountId, village);
            }
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Added update task"));
        }

        private async Task LoadAllHandler()
        {
            var getVillage = Locator.Current.GetService<GetVillage>();
            var villages = getVillage.All(AccountId);
            foreach (var village in villages)
            {
                await _taskManager.AddOrUpdate<UpdateBuildingTask>(AccountId, village);
            }
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Added update task"));
        }

        private static List<ListBoxItem> LoadVillageHandler(AccountId accountId)
        {
            var getVillage = Locator.Current.GetService<GetVillage>();
            return getVillage.Info(accountId);
        }
    }
}