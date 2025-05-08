using MainCore.Commands.UI.VillageViewModel;
using MainCore.UI.Enums;
using MainCore.UI.Models.Output;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.DependencyInjection;

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
            var serviceScopeFactory = Locator.Current.GetService<IServiceScopeFactory>();
            using var scope = serviceScopeFactory.CreateScope();
            var getVillageNameQuery = scope.ServiceProvider.GetRequiredService<GetVillageNameQuery.Handler>();
            var villageName = await getVillageNameQuery.HandleAsync(new(villageId));
            var taskManager = scope.ServiceProvider.GetRequiredService<ITaskManager>();
            await taskManager.AddOrUpdate<UpdateBuildingTask.Task>(new(AccountId, villageId, villageName));

            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Added update task"));
        }

        [ReactiveCommand]
        private async Task LoadUnload()
        {
            var serviceScopeFactory = Locator.Current.GetService<IServiceScopeFactory>();
            using var scope = serviceScopeFactory.CreateScope();
            var getMissingBuildingVillageQuery = scope.ServiceProvider.GetRequiredService<GetMissingBuildingVillagesQuery.Handler>();
            var villages = await getMissingBuildingVillageQuery.HandleAsync(new(AccountId));
            var taskManager = scope.ServiceProvider.GetRequiredService<ITaskManager>();
            var getVillageNameQuery = scope.ServiceProvider.GetRequiredService<GetVillageNameQuery.Handler>();
            foreach (var village in villages)
            {
                var villageName = await getVillageNameQuery.HandleAsync(new(village));
                await taskManager.AddOrUpdate<UpdateBuildingTask.Task>(new(AccountId, village, villageName));
            }
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Added update task"));
        }

        [ReactiveCommand]
        private async Task LoadAll()
        {
            var serviceScopeFactory = Locator.Current.GetService<IServiceScopeFactory>();
            using var scope = serviceScopeFactory.CreateScope();
            var getVillagesQuery = scope.ServiceProvider.GetRequiredService<GetVillagesQuery.Handler>();
            var villages = await getVillagesQuery.HandleAsync(new(AccountId));
            var taskManager = scope.ServiceProvider.GetRequiredService<ITaskManager>();
            var getVillageNameQuery = scope.ServiceProvider.GetRequiredService<GetVillageNameQuery.Handler>();
            foreach (var village in villages)
            {
                var villageName = await getVillageNameQuery.HandleAsync(new(village));
                await taskManager.AddOrUpdate<UpdateBuildingTask.Task>(new(AccountId, village, villageName));
            }
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Added update task"));
        }

        [ReactiveCommand]
        private static async Task<List<ListBoxItem>> LoadVillage(AccountId accountId)
        {
            var serviceScopeFactory = Locator.Current.GetService<IServiceScopeFactory>();
            using var scope = serviceScopeFactory.CreateScope();
            var getVillageItemsQuery = scope.ServiceProvider.GetRequiredService<GetVillageItemsQuery.Handler>();
            return await getVillageItemsQuery.HandleAsync(new(accountId));
        }
    }
}