using MainCore.Commands.UI.Villages.BuildViewModel;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton<BuildViewModel>]
    public partial class BuildViewModel : VillageTabViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IValidator<NormalBuildInput> _normalBuildInputValidator;
        private readonly IValidator<ResourceBuildInput> _resourceBuildInputValidator;

        public NormalBuildInput NormalBuildInput { get; } = new();
        public ResourceBuildInput ResourceBuildInput { get; } = new();

        public ListBoxItemViewModel Buildings { get; } = new();
        public ListBoxItemViewModel Queue { get; } = new();
        public ListBoxItemViewModel Jobs { get; } = new();

        public BuildViewModel(IDialogService dialogService, IValidator<NormalBuildInput> normalBuildInputValidator, IValidator<ResourceBuildInput> resourceBuildInputValidator)
        {
            _dialogService = dialogService;
            _normalBuildInputValidator = normalBuildInputValidator;
            _resourceBuildInputValidator = resourceBuildInputValidator;

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
        private static async Task<List<ListBoxItem>> LoadBuilding(VillageId villageId)
        {
            var getLayoutBuildingItemsQuery = Locator.Current.GetService<GetLayoutBuildingItemsQuery.Handler>();
            var items = await getLayoutBuildingItemsQuery.HandleAsync(new(villageId));
            return items;
        }

        [ReactiveCommand]
        private static async Task<List<ListBoxItem>> LoadQueue(VillageId villageId)
        {
            var getQueueBuildingItemsQuery = Locator.Current.GetService<GetQueueBuildingItemsQuery.Handler>();
            var items = await getQueueBuildingItemsQuery.HandleAsync(new(villageId));
            return items;
        }

        [ReactiveCommand]
        private static async Task<List<ListBoxItem>> LoadJob(VillageId villageId)
        {
            var getJobItemsQuery = Locator.Current.GetService<GetJobItemsQuery.Handler>();
            var jobs = await getJobItemsQuery.HandleAsync(new(villageId));
            return jobs;
        }

        [ReactiveCommand]
        private async Task<List<BuildingEnums>> LoadBuildNormal(ListBoxItem item)
        {
            if (item is null) return [];
            var getNormalBuildingsQuery = Locator.Current.GetService<GetNormalBuildingsQuery.Handler>();
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
            var normalBuildCommand = Locator.Current.GetService<NormalBuildCommand.Handler>();
            await normalBuildCommand.HandleAsync(new(AccountId, VillageId, NormalBuildInput.ToPlan(location)));
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
            var upgradeCommand = Locator.Current.GetService<UpgradeCommand.Handler>();
            await upgradeCommand.HandleAsync(new(AccountId, VillageId, location, false));
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
            var upgradeCommand = Locator.Current.GetService<UpgradeCommand.Handler>();
            await upgradeCommand.HandleAsync(new(AccountId, VillageId, location, true));
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

            var resourceBuildCommand = Locator.Current.GetService<ResourceBuildCommand.Handler>();
            await resourceBuildCommand.HandleAsync(new(AccountId, VillageId, ResourceBuildInput.ToPlan()));
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

            var swapCommand = Locator.Current.GetService<SwapCommand.Handler>();
            var newIndex = await swapCommand.HandleAsync(new(AccountId, VillageId, new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Up));
            Jobs.SelectedIndex = newIndex;
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

            var swapCommand = Locator.Current.GetService<SwapCommand.Handler>();
            var newIndex = await swapCommand.HandleAsync(new(AccountId, VillageId, new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Down));
            Jobs.SelectedIndex = newIndex;
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

            var moveCommand = Locator.Current.GetService<MoveCommand.Handler>();
            var newIndex = await moveCommand.HandleAsync(new(AccountId, VillageId, new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Top));
            Jobs.SelectedIndex = newIndex;
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

            var moveCommand = Locator.Current.GetService<MoveCommand.Handler>();
            var newIndex = await moveCommand.HandleAsync(new(AccountId, VillageId, new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Bottom));
            Jobs.SelectedIndex = newIndex;
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

            var deleteJobByIdCommand = Locator.Current.GetService<DeleteJobByIdCommand.Handler>();
            await deleteJobByIdCommand.HandleAsync(new(AccountId, VillageId, new JobId(jobId)));
        }

        [ReactiveCommand]
        private async Task DeleteAll()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }

            var deleteJobByVillageIdCommand = Locator.Current.GetService<DeleteJobByVillageIdCommand.Handler>();
            await deleteJobByVillageIdCommand.HandleAsync(new(AccountId, VillageId));
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

            var importCommand = Locator.Current.GetService<ImportCommand.Handler>();
            await importCommand.HandleAsync(new(AccountId, VillageId, jobs));
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
            var exportCommand = Locator.Current.GetService<ExportCommand.Handler>();
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