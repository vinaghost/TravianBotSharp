using MainCore.Commands.UI.VillageViewModel;
using MainCore.Specifications;
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
        private readonly ICustomServiceScopeFactory _serviceScopeFactory;
        public ListBoxItemViewModel Villages { get; } = new();

        public VillageTabStore VillageTabStore => _villageTabStore;

        public VillageViewModel(VillageTabStore villageTabStore, IDialogService dialogService, ICustomServiceScopeFactory serviceScopeFactory, IRxQueue rxQueue)
        {
            _villageTabStore = villageTabStore;
            _dialogService = dialogService;
            _serviceScopeFactory = serviceScopeFactory;

            var villageObservable = this.WhenAnyValue(x => x.Villages.SelectedItem);
            villageObservable.BindTo(_selectedItemStore, vm => vm.Village);
            villageObservable.Subscribe(x =>
            {
                var tabType = VillageTabType.Normal;
                if (x is null) tabType = VillageTabType.NoVillage;
                _villageTabStore.SetTabType(tabType);
            });

            LoadVillageCommand.Subscribe(Villages.Load);

            rxQueue.RegisterCommand(VillagesModifiedCommand);
        }

        [ReactiveCommand]
        public async Task VillagesModified(VillagesModified notification)
        {
            if (!IsActive) return;
            if (notification.AccountId != AccountId) return;
            await LoadVillageCommand.Execute(notification.AccountId);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadVillageCommand.Execute(accountId);
        }

        [ReactiveCommand]
        private async Task LoadCurrent()
        {
            if (Villages.SelectedItem is null)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "No village selected"));
                return;
            }

            var villageId = new VillageId(Villages.SelectedItem.Id);

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var villageName = context.GetVillageName(villageId);
            var taskManager = scope.ServiceProvider.GetRequiredService<ITaskManager>();
            taskManager.AddOrUpdate<UpdateBuildingTask.Task>(new(AccountId, villageId, villageName));

            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Added update task"));
        }

        [ReactiveCommand]
        private async Task LoadUnload()
        {
            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var missingBuildingVillagesSpec = new MissingBuildingVillagesSpec(AccountId);

            var villages = context.Villages
                .WithSpecification(missingBuildingVillagesSpec)
                .ToList();

            var taskManager = scope.ServiceProvider.GetRequiredService<ITaskManager>();

            foreach (var village in villages)
            {
                var villageName = context.GetVillageName(village);
                taskManager.AddOrUpdate<UpdateBuildingTask.Task>(new(AccountId, village, villageName));
            }

            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Added update task"));
        }

        [ReactiveCommand]
        private async Task LoadAll()
        {
            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var taskManager = scope.ServiceProvider.GetRequiredService<ITaskManager>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var villagesSpec = new VillagesSpec(AccountId);
            var villages = context.Villages
                .WithSpecification(villagesSpec)
                .ToList();
            foreach (var village in villages)
            {
                var villageName = context.GetVillageName(village);
                taskManager.AddOrUpdate<UpdateBuildingTask.Task>(new(AccountId, village, villageName));
            }
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Added update task"));
        }

        [ReactiveCommand]
        private async Task<List<ListBoxItem>> LoadVillage(AccountId accountId)
        {
            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var getVillageItemsQuery = scope.ServiceProvider.GetRequiredService<GetVillageItemsQuery.Handler>();
            return await getVillageItemsQuery.HandleAsync(new(accountId));
        }
    }
}