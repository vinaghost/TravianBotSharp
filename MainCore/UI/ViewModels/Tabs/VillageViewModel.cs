using MainCore.UI.Models.Output;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.UI.ViewModels.Tabs
{
    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Extensions;

    [RegisterSingleton<VillageViewModel>]
    public partial class VillageViewModel : AccountTabViewModelBase
    {
        private readonly VillageTabStore _villageTabStore;
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IRxQueue _rxQueue;
        public ListBoxItemViewModel Villages { get; } = new();

        public VillageTabStore VillageTabStore => _villageTabStore;

        public VillageViewModel(VillageTabStore villageTabStore, IDialogService dialogService, IRxQueue rxQueue, ITaskManager taskManager, IDbContextFactory<AppDbContext> contextFactory)
        {
            _villageTabStore = villageTabStore;
            _dialogService = dialogService;
            _rxQueue = rxQueue;
            _taskManager = taskManager;
            _contextFactory = contextFactory;

            Init();
        }

        private void Init()
        {
            var villageObservable = this.WhenAnyValue(x => x.Villages.SelectedItem);
            villageObservable.BindTo(_selectedItemStore, vm => vm.Village);
            villageObservable.Subscribe(x =>
            {
                if (x is null)
                {
                    _villageTabStore.SetTabType(VillageTabType.NoVillage);
                }
                else
                {
                    _villageTabStore.SetTabType(VillageTabType.Normal);
                }
            });

            LoadVillageCommand.Subscribe(Villages.Load);

            _rxQueue.GetObservable<VillagesModified>()
                .InvokeCommand(VillagesModifiedCommand);

            VillagesModifiedCommand
                .Where(x => x)
                .Select(_ => AccountId)
                .Throttle(TimeSpan.FromMilliseconds(1000), RxSchedulers.TaskpoolScheduler)
                .ObserveOn(RxSchedulers.TaskpoolScheduler)
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
            await LoadVillageCommand.Execute(accountId).ToHotTask();
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
            _taskManager.AddOrUpdate<UpdateBuildingTask.Task>(new(AccountId, villageId));

            await _dialogService.SendMessage("Information", $"Added update task");
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task LoadUnload()
        {
            using var context = _contextFactory.CreateDbContext();

            var villages = context.Villages
                .Where(x => x.AccountId == AccountId.Value)
                .Where(x => x.Buildings.Count < 40)
                .Select(x => new VillageId(x.Id))
                .ToList();

            foreach (var village in villages)
            {
                _taskManager.AddOrUpdate<UpdateBuildingTask.Task>(new(AccountId, village));
            }

            await _dialogService.SendMessage("Information", $"Added update task");
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task LoadAll()
        {
            using var context = _contextFactory.CreateDbContext();

            var villages = context.Villages
                .Where(x => x.AccountId == AccountId.Value)
                .Select(x => new VillageId(x.Id))
                .ToList();
            foreach (var village in villages)
            {
                _taskManager.AddOrUpdate<UpdateBuildingTask.Task>(new(AccountId, village));
            }
            await _dialogService.SendMessage("Information", $"Added update task");
        }

        [ReactiveCommand(RunInBackground = true)]
        private List<ListBoxItem> LoadVillage(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            var items = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .OrderBy(x => x.Name)
                .Select(x => new ListBoxItem()
                {
                    Id = x.Id,
                    Content = $"{x.Name}{Environment.NewLine}({x.X}|{x.Y})",
                })
                .ToList();
            return items;
        }
    }
}