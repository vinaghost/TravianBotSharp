using MainCore.Features.Update;
using MainCore.UI.Models.Output;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.UI.ViewModels.Tabs
{
    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Signals;

    [RegisterSingleton<VillageViewModel>]
    public partial class VillageViewModel : AccountTabViewModelBase
    {
        private readonly VillageTabStore _villageTabStore;
        private readonly TaskManager _taskManager;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly RxQueue _rxQueue;
        public ListBoxItemViewModel Villages { get; } = new();

        public VillageTabStore VillageTabStore => _villageTabStore;

        public VillageViewModel(VillageTabStore villageTabStore, DialogService dialogService, RxQueue rxQueue, TaskManager taskManager, IDbContextFactory<AppDbContext> contextFactory, SelectedItemStore selectedItemStore) : base(dialogService, selectedItemStore)
        {
            _villageTabStore = villageTabStore;
            _rxQueue = rxQueue;
            _taskManager = taskManager;
            _contextFactory = contextFactory;

            Init();
        }

        private void Init()
        {
            this.WhenAnyValue(x => x.Villages.SelectedItem)
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(100), RxSchedulers.MainThreadScheduler)
                .Subscribe(_selectedItemStore.SetVillage);

            LoadVillageCommand
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(x =>
                {
                    Villages.Load(x);

                    var oldSelectedVillage = _selectedItemStore.GetLastSelectedVillage(AccountId);
                    if (oldSelectedVillage != VillageId.Empty)
                    {
                        var item = Villages.Items.FirstOrDefault(v => v.Id == oldSelectedVillage.Value);
                        Villages.SelectedItem = item;
                    }

                    if (Villages.SelectedItem is null && Villages.Items.Count > 0)
                    {
                        Villages.SelectedItem = Villages.Items[0];
                    }
                });

            _rxQueue.GetObservable<VillagesModified>()
                .InvokeCommand(VillagesModifiedCommand);

            VillagesModifiedCommand
                .DistinctUntilChanged()
                .Where(x => x)
                .Select(_ => AccountId)
                .Throttle(TimeSpan.FromMilliseconds(1000), RxSchedulers.TaskpoolScheduler)
                .InvokeCommand(LoadVillageCommand);
        }

        [ReactiveCommand(RunInBackground = true)]
        public bool VillagesModified(VillagesModified notification)
        {
            if (!IsActive) return false;
            if (notification.AccountId != AccountId) return false;
            return true;
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadVillageCommand.Execute(accountId);
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task LoadCurrent()
        {
            if (Villages.SelectedItem is null)
            {
                await _dialogService.SendMessage("Warning", "No village selected");
                return;
            }

            var villageId = new VillageId(Villages.SelectedItem.Id);
            _taskManager.Add<UpdateBuildingTask.Task>(new(AccountId, villageId));

            await _dialogService.SendMessage("Information", $"Added update task to {Villages.SelectedItem.Content}");
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task LoadUnload()
        {
            var villages = GetMissingBuildingVillages(AccountId);

            foreach (var village in villages)
            {
                _taskManager.Add<UpdateBuildingTask.Task>(new(AccountId, village));
            }

            await _dialogService.SendMessage("Information", $"Added update task to {villages.Count} villages");
        }

        private List<VillageId> GetMissingBuildingVillages(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            return [.. context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.Buildings.Count < 40)
                .Select(x => new VillageId(x.Id))];
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task LoadAll()
        {
            List<VillageId> villages = GetVillages(AccountId);
            foreach (var village in villages)
            {
                _taskManager.Add<UpdateBuildingTask.Task>(new(AccountId, village));
            }
            await _dialogService.SendMessage("Information", "Added update task to all villages");
        }

        private List<VillageId> GetVillages(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            return [.. context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => new VillageId(x.Id))];
        }

        [ReactiveCommand(RunInBackground = true)]
        private List<ListBoxItem> LoadVillage(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var villages = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => new
                {
                    VillageId = x.Id,
                    x.Name,
                    x.X,
                    x.Y,
                })
                .ToList();

            var resourceFields = context.Buildings
                .Where(x => villages.Select(x => x.VillageId).Contains(x.VillageId))
                .Where(x => x.Type == BuildingEnums.Woodcutter || x.Type == BuildingEnums.ClayPit || x.Type == BuildingEnums.IronMine || x.Type == BuildingEnums.Cropland)
                .GroupBy(x => x.VillageId)
                .ToDictionary(x => x.Key, x => x.Select(b => b.Type));

            List<ListBoxItem> items = [];

            foreach (var village in villages.OrderBy(x => x.Name))
            {
                if (resourceFields.TryGetValue(village.VillageId, out var buildings) && buildings.Any())
                {
                    items.Add(new ListBoxItem()
                    {
                        Id = village.VillageId,
                        Content = $"{village.Name}{Environment.NewLine}({village.X}|{village.Y}) [{GetVillageType([.. buildings])}]",
                    });
                }
                else
                {
                    items.Add(new ListBoxItem()
                    {
                        Id = village.VillageId,
                        Content = $"{village.Name}{Environment.NewLine}({village.X}|{village.Y})",
                    });
                }
            }
            return items;
        }

        private static string GetVillageType(List<BuildingEnums> buildings)
        {
            var woodCount = buildings.Count(x => x == BuildingEnums.Woodcutter);
            var clayCount = buildings.Count(x => x == BuildingEnums.ClayPit);
            var ironCount = buildings.Count(x => x == BuildingEnums.IronMine);
            var cropCount = buildings.Count(x => x == BuildingEnums.Cropland);
            if (cropCount == 0) return "Not loaded";
            if (cropCount == 15) return "15C";
            if (cropCount == 9) return "9C";
            return $"{woodCount}{clayCount}{ironCount}{cropCount}";
        }
    }
}