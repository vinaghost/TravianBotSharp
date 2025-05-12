using MainCore.Commands.UI.Villages.BuildViewModel;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton<BuildViewModel>]
    public partial class BuildViewModel : VillageTabViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IValidator<NormalBuildInput> _normalBuildInputValidator;
        private readonly IValidator<ResourceBuildInput> _resourceBuildInputValidator;
        private readonly ICustomServiceScopeFactory _serviceScopeFactory;

        public NormalBuildInput NormalBuildInput { get; } = new();
        public ResourceBuildInput ResourceBuildInput { get; } = new();

        public ListBoxItemViewModel Buildings { get; } = new();
        public ListBoxItemViewModel Queue { get; } = new();
        public ListBoxItemViewModel Jobs { get; } = new();

        public BuildViewModel(IDialogService dialogService, IValidator<NormalBuildInput> normalBuildInputValidator, IValidator<ResourceBuildInput> resourceBuildInputValidator, ICustomServiceScopeFactory serviceScopeFactory)
        {
            _dialogService = dialogService;
            _normalBuildInputValidator = normalBuildInputValidator;
            _resourceBuildInputValidator = resourceBuildInputValidator;
            _serviceScopeFactory = serviceScopeFactory;

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
        private async Task<List<ListBoxItem>> LoadBuilding(VillageId villageId)
        {
            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var getLayoutBuildingItemsQuery = scope.ServiceProvider.GetRequiredService<GetLayoutBuildingItemsQuery.Handler>();
            var items = await getLayoutBuildingItemsQuery.HandleAsync(new(villageId));
            return items;
        }

        [ReactiveCommand]
        private async Task<List<ListBoxItem>> LoadQueue(VillageId villageId)
        {
            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var getQueueBuildingItemsQuery = scope.ServiceProvider.GetRequiredService<GetQueueBuildingItemsQuery.Handler>();
            var items = await getQueueBuildingItemsQuery.HandleAsync(new(villageId));
            return items;
        }

        [ReactiveCommand]
        private async Task<List<ListBoxItem>> LoadJob(VillageId villageId)
        {
            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var getJobItemsQuery = scope.ServiceProvider.GetRequiredService<GetJobItemsQuery.Handler>();
            var jobs = await getJobItemsQuery.HandleAsync(new(villageId));
            return jobs;
        }

        [ReactiveCommand]
        private async Task<List<BuildingEnums>> LoadBuildNormal(ListBoxItem item)
        {
            if (item is null) return [];

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var getNormalBuildingsQuery = scope.ServiceProvider.GetRequiredService<GetNormalBuildingsQuery.Handler>();
            var items = await getNormalBuildingsQuery.HandleAsync(new(VillageId, new BuildingId(item.Id)));
            return items;
        }

        [ReactiveCommand]
        private async Task BuildNormal()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }

            var result = await _normalBuildInputValidator.ValidateAsync(NormalBuildInput);
            if (!result.IsValid)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
                return;
            }

            var location = Buildings.SelectedIndex + 1;

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var normalBuildCommand = scope.ServiceProvider.GetRequiredService<NormalBuildCommand.Handler>();
            var buildResult = await normalBuildCommand.HandleAsync(new(VillageId, NormalBuildInput.ToPlan(location)));
            if (buildResult.IsFailed)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", buildResult.ToString()));
                return;
            }
            var jobUpdated = scope.ServiceProvider.GetRequiredService<JobUpdated.Handler>();
            await jobUpdated.HandleAsync(new(AccountId, VillageId));
        }

        [ReactiveCommand]
        private async Task UpgradeOneLevel()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            var location = Buildings.SelectedIndex + 1;

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var upgradeCommand = scope.ServiceProvider.GetRequiredService<UpgradeCommand.Handler>();
            await upgradeCommand.HandleAsync(new(VillageId, location, false));
            var jobUpdated = scope.ServiceProvider.GetRequiredService<JobUpdated.Handler>();
            await jobUpdated.HandleAsync(new(AccountId, VillageId));
        }

        [ReactiveCommand]
        private async Task UpgradeMaxLevel()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            var location = Buildings.SelectedIndex + 1;

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var upgradeCommand = scope.ServiceProvider.GetRequiredService<UpgradeCommand.Handler>();
            await upgradeCommand.HandleAsync(new(VillageId, location, true));
            var jobUpdated = scope.ServiceProvider.GetRequiredService<JobUpdated.Handler>();
            await jobUpdated.HandleAsync(new(AccountId, VillageId));
        }

        [ReactiveCommand]
        private async Task BuildResource()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }

            var result = await _resourceBuildInputValidator.ValidateAsync(ResourceBuildInput);
            if (!result.IsValid)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var resourceBuildCommand = scope.ServiceProvider.GetRequiredService<ResourceBuildCommand.Handler>();
            await resourceBuildCommand.HandleAsync(new(VillageId, ResourceBuildInput.ToPlan()));
            var jobUpdated = scope.ServiceProvider.GetRequiredService<JobUpdated.Handler>();
            await jobUpdated.HandleAsync(new(AccountId, VillageId));
        }

        [ReactiveCommand]
        private async Task Up()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }

            if (!Jobs.IsSelected)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please select before moving"));
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var swapCommand = scope.ServiceProvider.GetRequiredService<SwapCommand.Handler>();
            var newIndex = await swapCommand.HandleAsync(new(VillageId, new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Up));
            Jobs.SelectedIndex = newIndex;

            var jobUpdated = scope.ServiceProvider.GetRequiredService<JobUpdated.Handler>();
            await jobUpdated.HandleAsync(new(AccountId, VillageId));
        }

        [ReactiveCommand]
        private async Task Down()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            if (!Jobs.IsSelected)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please select before moving"));
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var swapCommand = scope.ServiceProvider.GetRequiredService<SwapCommand.Handler>();
            var newIndex = await swapCommand.HandleAsync(new(VillageId, new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Down));
            Jobs.SelectedIndex = newIndex;
            var jobUpdated = scope.ServiceProvider.GetRequiredService<JobUpdated.Handler>();
            await jobUpdated.HandleAsync(new(AccountId, VillageId));
        }

        [ReactiveCommand]
        private async Task Top()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            if (!Jobs.IsSelected)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please select before moving"));
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var moveCommand = scope.ServiceProvider.GetRequiredService<MoveCommand.Handler>();
            var newIndex = await moveCommand.HandleAsync(new(VillageId, new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Top));
            Jobs.SelectedIndex = newIndex;

            var jobUpdated = scope.ServiceProvider.GetRequiredService<JobUpdated.Handler>();
            await jobUpdated.HandleAsync(new(AccountId, VillageId));
        }

        [ReactiveCommand]
        private async Task Bottom()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            if (!Jobs.IsSelected)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please select before moving"));
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var moveCommand = scope.ServiceProvider.GetRequiredService<MoveCommand.Handler>();
            var newIndex = await moveCommand.HandleAsync(new(VillageId, new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Bottom));
            Jobs.SelectedIndex = newIndex;
            var jobUpdated = scope.ServiceProvider.GetRequiredService<JobUpdated.Handler>();
            await jobUpdated.HandleAsync(new(AccountId, VillageId));
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

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var deleteJobByIdCommand = scope.ServiceProvider.GetRequiredService<DeleteJobByIdCommand.Handler>();
            await deleteJobByIdCommand.HandleAsync(new(VillageId, new JobId(jobId)));
            var jobUpdated = scope.ServiceProvider.GetRequiredService<JobUpdated.Handler>();
            await jobUpdated.HandleAsync(new(AccountId, VillageId));
        }

        [ReactiveCommand]
        private async Task DeleteAll()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var deleteJobByVillageIdCommand = scope.ServiceProvider.GetRequiredService<DeleteJobByVillageIdCommand.Handler>();
            await deleteJobByVillageIdCommand.HandleAsync(new(VillageId));

            var jobUpdated = scope.ServiceProvider.GetRequiredService<JobUpdated.Handler>();
            await jobUpdated.HandleAsync(new(AccountId, VillageId));
        }

        [ReactiveCommand]
        private async Task Import()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            var path = await _dialogService.OpenFileDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(path)) return;
            List<JobDto> jobs;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path);
                jobs = JsonSerializer.Deserialize<List<JobDto>>(jsonString);
            }
            catch
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Invalid file."));
                return;
            }

            var confirm = await _dialogService.ConfirmBox.Handle(new MessageBoxData("Warning", "TBS will remove resource field build job if its position doesn't match with current village."));
            if (!confirm) return;

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var fixJobsCommand = scope.ServiceProvider.GetRequiredService<FixJobsCommand.Handler>();
            var fixedJobs = await fixJobsCommand.HandleAsync(new(VillageId, jobs));
            var importCommand = scope.ServiceProvider.GetRequiredService<ImportCommand.Handler>();
            await importCommand.HandleAsync(new(VillageId, fixedJobs));
            var jobUpdated = scope.ServiceProvider.GetRequiredService<JobUpdated.Handler>();
            await jobUpdated.HandleAsync(new(AccountId, VillageId));
        }

        [ReactiveCommand]
        private async Task Export()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }

            var path = await _dialogService.SaveFileDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(path)) return;

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var exportCommand = scope.ServiceProvider.GetRequiredService<ExportCommand.Handler>();
            await exportCommand.HandleAsync(new(VillageId, path));
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Job list exported"));
        }

        private bool IsAccountPaused(AccountId accountId)
        {
            var taskManager = Locator.Current.GetService<ITaskManager>();
            var status = taskManager.GetStatus(accountId);
            if (status == StatusEnums.Online)
            {
                return false;
            }
            return true;
        }
    }
}