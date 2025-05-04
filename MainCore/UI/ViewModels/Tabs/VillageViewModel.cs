using MainCore.Commands.UI.VillageViewModel;
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

        private readonly IDialogService _dialogService;
        public ListBoxItemViewModel Villages { get; } = new();

        public VillageTabStore VillageTabStore => _villageTabStore;

        public VillageViewModel(VillageTabStore villageTabStore, IDialogService dialogService)
        {
            _villageTabStore = villageTabStore;
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
            var taskManager = Locator.Current.GetService<ITaskManager>();
            await taskManager.AddOrUpdate<UpdateBuildingTask.Task>(AccountId, villageId);

            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Added update task"));
        }

        [ReactiveCommand]
        private async Task LoadUnload()
        {
            var getMissingBuildingVillageQuery = Locator.Current.GetService<GetMissingBuildingVillagesQuery.Handler>();
            var villages = await getMissingBuildingVillageQuery.HandleAsync(new(AccountId));
            var taskManager = Locator.Current.GetService<ITaskManager>();
            foreach (var village in villages)
            {
                await taskManager.AddOrUpdate<UpdateBuildingTask.Task>(AccountId, village);
            }
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Added update task"));
        }

        [ReactiveCommand]
        private async Task LoadAll()
        {
            var getVillagesQuery = Locator.Current.GetService<GetVillagesQuery.Handler>();
            var villages = await getVillagesQuery.HandleAsync(new(AccountId));
            var taskManager = Locator.Current.GetService<ITaskManager>();
            foreach (var village in villages)
            {
                await taskManager.AddOrUpdate<UpdateBuildingTask.Task>(AccountId, village);
            }
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Added update task"));
        }

        [ReactiveCommand]
        private static async Task<List<ListBoxItem>> LoadVillage(AccountId accountId)
        {
            var getVillageItemsQuery = Locator.Current.GetService<GetVillageItemsQuery.Handler>();
            return await getVillageItemsQuery.HandleAsync(new(accountId));
        }
    }
}