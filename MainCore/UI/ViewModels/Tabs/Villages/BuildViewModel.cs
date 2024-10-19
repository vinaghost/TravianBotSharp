using MainCore.Commands.UI.Villages;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Linq;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton<BuildViewModel>]
    public class BuildViewModel : VillageTabViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;

        private ReactiveCommand<ListBoxItem, List<BuildingEnums>> LoadBuildNormal { get; }

        public ReactiveCommand<Unit, Unit> BuildNormal { get; }
        public ReactiveCommand<Unit, Unit> BuildResource { get; }
        public ReactiveCommand<Unit, Unit> UpgradeOneLevel { get; }
        public ReactiveCommand<Unit, Unit> UpgradeMaxLevel { get; }

        public ReactiveCommand<Unit, Unit> Up { get; }
        public ReactiveCommand<Unit, Unit> Down { get; }
        public ReactiveCommand<Unit, Unit> Top { get; }
        public ReactiveCommand<Unit, Unit> Bottom { get; }
        public ReactiveCommand<Unit, Unit> Delete { get; }
        public ReactiveCommand<Unit, Unit> DeleteAll { get; }
        public ReactiveCommand<Unit, Unit> Import { get; }
        public ReactiveCommand<Unit, Unit> Export { get; }
        public ReactiveCommand<VillageId, List<ListBoxItem>> LoadBuilding { get; }
        public ReactiveCommand<VillageId, List<ListBoxItem>> LoadJob { get; }
        public ReactiveCommand<VillageId, List<ListBoxItem>> LoadQueue { get; }

        public NormalBuildInput NormalBuildInput { get; } = new();

        public ResourceBuildInput ResourceBuildInput { get; } = new();

        public ListBoxItemViewModel Buildings { get; } = new();
        public ListBoxItemViewModel Queue { get; } = new();
        public ListBoxItemViewModel Jobs { get; } = new();

        public BuildViewModel(IMediator mediator, IDialogService dialogService, ITaskManager taskManager)
        {
            _mediator = mediator;
            _dialogService = dialogService;
            _taskManager = taskManager;

            BuildNormal = ReactiveCommand.CreateFromTask(BuildNormalHandler);
            BuildResource = ReactiveCommand.CreateFromTask(ResourceNormalHandler);
            UpgradeOneLevel = ReactiveCommand.CreateFromTask(UpgradeOneLevelHandler);
            UpgradeMaxLevel = ReactiveCommand.CreateFromTask(UpgradeMaxLevelHandler);

            Up = ReactiveCommand.CreateFromTask(UpHandler);
            Down = ReactiveCommand.CreateFromTask(DownHandler);
            Top = ReactiveCommand.CreateFromTask(TopHandler);
            Bottom = ReactiveCommand.CreateFromTask(BottomHandler);
            Delete = ReactiveCommand.CreateFromTask(DeleteHandler);
            DeleteAll = ReactiveCommand.CreateFromTask(DeleteAllHandler);
            Import = ReactiveCommand.CreateFromTask(ImportHandler);
            Export = ReactiveCommand.CreateFromTask(ExportHandler);

            LoadBuilding = ReactiveCommand.Create<VillageId, List<ListBoxItem>>(LoadBuildingHandler);
            LoadJob = ReactiveCommand.Create<VillageId, List<ListBoxItem>>(LoadJobHandler);
            LoadQueue = ReactiveCommand.Create<VillageId, List<ListBoxItem>>(LoadQueueHandler);
            LoadBuildNormal = ReactiveCommand.Create<ListBoxItem, List<BuildingEnums>>(LoadBuildNormalHanlder);

            this.WhenAnyValue(vm => vm.Buildings.SelectedItem)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .InvokeCommand(LoadBuildNormal);

            LoadBuilding.Subscribe(Buildings.Load);
            LoadJob.Subscribe(Jobs.Load);
            LoadQueue.Subscribe(Queue.Load);

            LoadBuildNormal.Subscribe(buildings =>
            {
                switch (buildings.Count)
                {
                    case 0:
                        NormalBuildInput.Clear();
                        break;

                    default:
                        NormalBuildInput.Set(buildings, -1);
                        break;
                }
            });
        }

        public async Task QueueRefresh(VillageId villageId)
        {
            if (!IsActive) return;
            if (villageId != VillageId) return;
            await LoadQueue.Execute(villageId);
        }

        public async Task BuildingListRefresh(VillageId villageId)
        {
            if (!IsActive) return;
            if (villageId != VillageId) return;
            await LoadBuilding.Execute(villageId);
        }

        public async Task JobListRefresh(VillageId villageId)
        {
            if (!IsActive) return;
            if (villageId != VillageId) return;
            await LoadJob.Execute(villageId);
            await LoadBuilding.Execute(villageId);
        }

        protected override async Task Load(VillageId villageId)
        {
            await LoadJob.Execute(villageId);
            await LoadBuilding.Execute(villageId);
            await LoadQueue.Execute(villageId);
        }

        private static List<ListBoxItem> LoadBuildingHandler(VillageId villageId)
        {
            var getBuildings = Locator.Current.GetService<GetBuildings>();
            var items = getBuildings.LayoutItems(villageId);
            return items;
        }

        private static List<ListBoxItem> LoadQueueHandler(VillageId villageId)
        {
            var getBuildings = Locator.Current.GetService<GetBuildings>();
            var items = getBuildings.QueueItems(villageId);
            return items;
        }

        private static List<ListBoxItem> LoadJobHandler(VillageId villageId)
        {
            var getJobs = Locator.Current.GetService<GetJobs>();
            var jobs = getJobs.Items(villageId);
            return jobs;
        }

        private List<BuildingEnums> LoadBuildNormalHanlder(ListBoxItem item)
        {
            if (item is null) return [];
            var getBuildings = Locator.Current.GetService<GetBuildings>();
            var buildings = getBuildings.NormalBuilds(VillageId, new BuildingId(item.Id));
            return buildings;
        }

        private async Task BuildNormalHandler()
        {
            if (!IsAccountPaused(AccountId)) return;

            var buildNormalCommand = Locator.Current.GetService<BuildNormalCommand>();
            var location = Buildings.SelectedIndex + 1;
            await buildNormalCommand.Execute(AccountId, VillageId, NormalBuildInput, location);
        }

        private async Task UpgradeOneLevelHandler()
        {
            if (!IsAccountPaused(AccountId)) return;

            var upgradeLevel = Locator.Current.GetService<UpgradeLevel>();
            var location = Buildings.SelectedIndex + 1;
            await upgradeLevel.Execute(AccountId, VillageId, location, false);
        }

        private async Task UpgradeMaxLevelHandler()
        {
            if (!IsAccountPaused(AccountId)) return;

            var upgradeLevel = Locator.Current.GetService<UpgradeLevel>();
            var location = Buildings.SelectedIndex + 1;
            await upgradeLevel.Execute(AccountId, VillageId, location, true);
        }

        private async Task ResourceNormalHandler()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }

            var buildResourceCommand = Locator.Current.GetService<BuildResourceCommand>();
            await buildResourceCommand.Execute(AccountId, VillageId, ResourceBuildInput);
        }

        private async Task UpHandler()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            var moveJobCommand = Locator.Current.GetService<MoveJobCommand>();
            await moveJobCommand.Execute(AccountId, VillageId, Jobs, MoveEnums.Up);
        }

        private async Task DownHandler()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            var moveJobCommand = Locator.Current.GetService<MoveJobCommand>();
            await moveJobCommand.Execute(AccountId, VillageId, Jobs, MoveEnums.Down);
        }

        private async Task TopHandler()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            var moveJobCommand = Locator.Current.GetService<MoveJobCommand>();
            await moveJobCommand.Execute(AccountId, VillageId, Jobs, MoveEnums.Top);
        }

        private async Task BottomHandler()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            var moveJobCommand = Locator.Current.GetService<MoveJobCommand>();
            await moveJobCommand.Execute(AccountId, VillageId, Jobs, MoveEnums.Bottom);
        }

        private async Task DeleteHandler()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            if (!Jobs.IsSelected) return;
            var jobId = Jobs.SelectedItemId;

            var deleteJobCommand = Locator.Current.GetService<DeleteJobCommand>();
            deleteJobCommand.ByJobId(new JobId(jobId));
            await _mediator.Publish(new JobUpdated(AccountId, VillageId));
        }

        private async Task DeleteAllHandler()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            var deleteJobCommand = Locator.Current.GetService<DeleteJobCommand>();
            deleteJobCommand.ByVillageId(VillageId);
            await _mediator.Publish(new JobUpdated(AccountId, VillageId));
        }

        private async Task ImportHandler()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            var importCommand = Locator.Current.GetService<ImportCommand>();
            await importCommand.Execute(AccountId, VillageId);
        }

        private async Task ExportHandler()
        {
            var path = await _dialogService.SaveFileDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(path)) return;
            var getJobs = Locator.Current.GetService<GetJobs>();
            var jobs = getJobs.Dtos(VillageId);
            jobs.ForEach(job => job.Id = JobId.Empty);
            var jsonString = JsonSerializer.Serialize(jobs);
            await File.WriteAllTextAsync(path, jsonString);
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Job list exported"));
        }

        private bool IsAccountPaused(AccountId accountId)
        {
            var status = _taskManager.GetStatus(accountId);
            if (status == StatusEnums.Online)
            {
                return false;
            }
            return true;
        }
    }
}