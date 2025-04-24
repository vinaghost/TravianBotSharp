using MainCore.Commands.UI.Villages;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton<BuildViewModel>]
    public partial class BuildViewModel : VillageTabViewModelBase
    {
        private readonly JobUpdated.Handler _jobUpdated;
        private readonly IDialogService _dialogService;
        private readonly ITimerManager _timerManager;

        public NormalBuildInput NormalBuildInput { get; } = new();

        public ResourceBuildInput ResourceBuildInput { get; } = new();

        public ListBoxItemViewModel Buildings { get; } = new();
        public ListBoxItemViewModel Queue { get; } = new();
        public ListBoxItemViewModel Jobs { get; } = new();

        public BuildViewModel(IDialogService dialogService, ITimerManager timerManager, JobUpdated.Handler jobUpdated)
        {
            _dialogService = dialogService;
            _timerManager = timerManager;
            _jobUpdated = jobUpdated;

            this.WhenAnyValue(vm => vm.Buildings.SelectedItem)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .InvokeCommand(LoadBuildNormalCommand);

            LoadBuildingCommand.Subscribe(Buildings.Load);
            LoadJobCommand.Subscribe(Jobs.Load);
            LoadQueueCommand.Subscribe(Queue.Load);

            LoadBuildNormalCommand.Subscribe(buildings =>
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
            await LoadQueueCommand.Execute(villageId);
        }

        public async Task BuildingListRefresh(VillageId villageId)
        {
            if (!IsActive) return;
            if (villageId != VillageId) return;
            await LoadBuildingCommand.Execute(villageId);
        }

        public async Task JobListRefresh(VillageId villageId)
        {
            if (!IsActive) return;
            if (villageId != VillageId) return;
            await LoadJobCommand.Execute(villageId);
            await LoadBuildingCommand.Execute(villageId);
        }

        protected override async Task Load(VillageId villageId)
        {
            await LoadJobCommand.Execute(villageId);
            await LoadBuildingCommand.Execute(villageId);
            await LoadQueueCommand.Execute(villageId);
        }

        [ReactiveCommand]
        private static List<ListBoxItem> LoadBuilding(VillageId villageId)
        {
            var getBuildings = Locator.Current.GetService<GetBuildings>();
            var items = getBuildings.LayoutItems(villageId);
            return items;
        }

        [ReactiveCommand]
        private static List<ListBoxItem> LoadQueue(VillageId villageId)
        {
            var getBuildings = Locator.Current.GetService<GetBuildings>();
            var items = getBuildings.QueueItems(villageId);
            return items;
        }

        [ReactiveCommand]
        private static List<ListBoxItem> LoadJob(VillageId villageId)
        {
            var getJobs = Locator.Current.GetService<GetJobs>();
            var jobs = getJobs.Items(villageId);
            return jobs;
        }

        [ReactiveCommand]
        private List<BuildingEnums> LoadBuildNormal(ListBoxItem item)
        {
            if (item is null) return [];
            var getBuildings = Locator.Current.GetService<GetBuildings>();
            var buildings = getBuildings.NormalBuilds(VillageId, new BuildingId(item.Id));
            return buildings;
        }

        [ReactiveCommand]
        private async Task BuildNormal()
        {
            if (!IsAccountPaused(AccountId)) return;

            var buildNormalCommand = Locator.Current.GetService<BuildNormalCommand>();
            var location = Buildings.SelectedIndex + 1;
            await buildNormalCommand.Execute(AccountId, VillageId, NormalBuildInput, location);
        }

        [ReactiveCommand]
        private async Task UpgradeOneLevel()
        {
            if (!IsAccountPaused(AccountId)) return;

            var upgradeLevel = Locator.Current.GetService<UpgradeLevel>();
            var location = Buildings.SelectedIndex + 1;
            await upgradeLevel.Execute(AccountId, VillageId, location, false);
        }

        [ReactiveCommand]
        private async Task UpgradeMaxLevel()
        {
            if (!IsAccountPaused(AccountId)) return;

            var upgradeLevel = Locator.Current.GetService<UpgradeLevel>();
            var location = Buildings.SelectedIndex + 1;
            await upgradeLevel.Execute(AccountId, VillageId, location, true);
        }

        [ReactiveCommand]
        private async Task BuildResource()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }

            var buildResourceCommand = Locator.Current.GetService<BuildResourceCommand>();
            await buildResourceCommand.Execute(AccountId, VillageId, ResourceBuildInput);
        }

        [ReactiveCommand]
        private async Task Up()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            var moveJobCommand = Locator.Current.GetService<MoveJobCommand>();
            await moveJobCommand.Execute(AccountId, VillageId, Jobs, MoveEnums.Up);
        }

        [ReactiveCommand]
        private async Task Down()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            var moveJobCommand = Locator.Current.GetService<MoveJobCommand>();
            await moveJobCommand.Execute(AccountId, VillageId, Jobs, MoveEnums.Down);
        }

        [ReactiveCommand]
        private async Task Top()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            var moveJobCommand = Locator.Current.GetService<MoveJobCommand>();
            await moveJobCommand.Execute(AccountId, VillageId, Jobs, MoveEnums.Top);
        }

        [ReactiveCommand]
        private async Task Bottom()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            var moveJobCommand = Locator.Current.GetService<MoveJobCommand>();
            await moveJobCommand.Execute(AccountId, VillageId, Jobs, MoveEnums.Bottom);
        }

        [ReactiveCommand]
        private async Task Delete()
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
            await _jobUpdated.HandleAsync(new(AccountId, VillageId));
        }

        [ReactiveCommand]
        private async Task DeleteAll()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            var deleteJobCommand = Locator.Current.GetService<DeleteJobCommand>();
            deleteJobCommand.ByVillageId(VillageId);
            await _jobUpdated.HandleAsync(new(AccountId, VillageId));
        }

        [ReactiveCommand]
        private async Task Import()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            var importCommand = Locator.Current.GetService<ImportCommand>();
            await importCommand.Execute(AccountId, VillageId);
        }

        [ReactiveCommand]
        private async Task Export()
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
            var status = _timerManager.GetStatus(accountId);
            if (status == StatusEnums.Online)
            {
                return false;
            }
            return true;
        }
    }
}