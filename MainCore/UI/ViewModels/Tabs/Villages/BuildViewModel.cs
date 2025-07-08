using Humanizer;
using MainCore.Commands.UI.Villages.BuildViewModel;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton<BuildViewModel>]
    public partial class BuildViewModel : VillageTabViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;
        private readonly IValidator<NormalBuildInput> _normalBuildInputValidator;
        private readonly IValidator<ResourceBuildInput> _resourceBuildInputValidator;
        private readonly ICustomServiceScopeFactory _serviceScopeFactory;

        public NormalBuildInput NormalBuildInput { get; } = new();
        public ResourceBuildInput ResourceBuildInput { get; } = new();

        public ListBoxItemViewModel Buildings { get; } = new();
        public ListBoxItemViewModel Queue { get; } = new();
        public ListBoxItemViewModel Jobs { get; } = new();

        public BuildViewModel(IDialogService dialogService, IValidator<NormalBuildInput> normalBuildInputValidator, IValidator<ResourceBuildInput> resourceBuildInputValidator, ICustomServiceScopeFactory serviceScopeFactory, ITaskManager taskManager, IRxQueue rxQueue)
        {
            _dialogService = dialogService;
            _normalBuildInputValidator = normalBuildInputValidator;
            _resourceBuildInputValidator = resourceBuildInputValidator;
            _serviceScopeFactory = serviceScopeFactory;
            _taskManager = taskManager;

            this.WhenAnyValue(vm => vm.Buildings.SelectedItem)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .WhereNotNull()
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

            rxQueue.RegisterCommand<BuildingsModified>(BuildingsModifiedCommand);
            rxQueue.RegisterCommand<JobsModified>(JobsModifiedCommand);
        }

        [ReactiveCommand]
        public async Task BuildingsModified(BuildingsModified notification)
        {
            if (!IsActive) return;
            if (notification.VillageId != VillageId) return;
            await LoadQueueCommand.Execute(notification.VillageId);
            await LoadBuildingCommand.Execute(notification.VillageId);
        }

        [ReactiveCommand]
        public async Task JobsModified(JobsModified notification)
        {
            if (!IsActive) return;
            if (notification.VillageId != VillageId) return;
            await LoadJobCommand.Execute(notification.VillageId);
            await LoadBuildingCommand.Execute(notification.VillageId);
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
            var getLayoutBuildingsQuery = scope.ServiceProvider.GetRequiredService<GetLayoutBuildingsCommand.Handler>();
            var buildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId));
            static ListBoxItem ToListBoxItem(BuildingItem building)
            {
                const string arrow = " -> ";
                var sb = new StringBuilder();
                sb.Append(building.CurrentLevel);
                if (building.QueueLevel != 0)
                {
                    var content = $"{arrow}({building.QueueLevel})";
                    sb.Append(content);
                }
                if (building.JobLevel != 0 && building.JobLevel > building.CurrentLevel)
                {
                    var content = $"{arrow}[{building.JobLevel}]";
                    sb.Append(content);
                }

                var item = new ListBoxItem()
                {
                    Id = building.Id.Value,
                    Content = $"[{building.Location}] {building.Type.Humanize()} | lvl {sb}",
                    Color = building.Type.GetColor(),
                };
                return item;
            }
            var items = buildings
                .Select(ToListBoxItem)
                .ToList();
            return items;
        }

        [ReactiveCommand]
        private List<ListBoxItem> LoadQueue(VillageId villageId)
        {
            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var items = context.QueueBuildings
                 .Where(x => x.VillageId == villageId.Value)
                 .AsEnumerable()
                 .Select(x => new ListBoxItem()
                 {
                     Id = x.Id,
                     Content = $"{x.Type.Humanize()} to level {x.Level} complete at {x.CompleteTime}",
                 })
                 .ToList();

            var tribe = (TribeEnums)context.VillagesSetting
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Setting == VillageSettingEnums.Tribe)
                .Select(x => x.Value)
                .FirstOrDefault();

            var count = 2;
            if (tribe == TribeEnums.Romans) count = 3;
            items.AddRange(Enumerable.Range(0, Math.Max(count - items.Count, 0)).Select((x) => new ListBoxItem() { Id = x - 5 }));
            return items;
        }

        [ReactiveCommand]
        private List<ListBoxItem> LoadJob(VillageId villageId)
        {
            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var items = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .OrderBy(x => x.Position)
                .ToDto()
                .AsEnumerable()
                .Select(x => new ListBoxItem()
                {
                    Id = x.Id.Value,
                    Content = x.ToString(),
                })
                .ToList();

            return items;
        }

        private static readonly List<BuildingEnums> MultipleBuildings =
        [
            BuildingEnums.Warehouse,
            BuildingEnums.Granary,
            BuildingEnums.Trapper,
            BuildingEnums.Cranny,
        ];

        private static readonly List<BuildingEnums> IgnoreBuildings =
        [
            BuildingEnums.Site,
            BuildingEnums.Blacksmith,
            BuildingEnums.CityWall,
            BuildingEnums.EarthWall,
            BuildingEnums.Palisade,
            BuildingEnums.WW,
            BuildingEnums.StoneWall,
            BuildingEnums.MakeshiftWall,
            BuildingEnums.Unknown,
        ];

        private static readonly List<BuildingEnums> AvailableBuildings = Enum.GetValues(typeof(BuildingEnums))
            .Cast<BuildingEnums>()
            .Where(x => !IgnoreBuildings.Contains(x))
            .ToList();

        [ReactiveCommand]
        private async Task<List<BuildingEnums>> LoadBuildNormal(ListBoxItem item)
        {
            if (item is null) return [];

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var getLayoutBuildingsQuery = scope.ServiceProvider.GetRequiredService<GetLayoutBuildingsCommand.Handler>();
            var buildingItems = await getLayoutBuildingsQuery.HandleAsync(new(VillageId));

            var type = buildingItems
                .Where(x => x.Id == new BuildingId(item.Id))
                .Select(x => x.Type)
                .FirstOrDefault();

            if (type != BuildingEnums.Site) return [type];

            var buildings = buildingItems
                .Select(x => x.Type)
                .Where(x => !MultipleBuildings.Contains(x))
                .Distinct()
                .ToList();

            return AvailableBuildings.Where(x => !buildings.Contains(x)).ToList();
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

            await JobsModifiedCommand.Execute(new JobsModified(VillageId));
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
            await JobsModifiedCommand.Execute(new JobsModified(VillageId));
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
            await JobsModifiedCommand.Execute(new JobsModified(VillageId));
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
            await JobsModifiedCommand.Execute(new JobsModified(VillageId));
        }

        [ReactiveCommand]
        private async Task Up()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }

            if (Jobs.SelectedItem is null)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please select before moving"));
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var swapCommand = scope.ServiceProvider.GetRequiredService<SwapCommand.Handler>();
            var newIndex = await swapCommand.HandleAsync(new(new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Up));
            Jobs.SelectedIndex = newIndex;

            await JobsModifiedCommand.Execute(new JobsModified(VillageId));
        }

        [ReactiveCommand]
        private async Task Down()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            if (Jobs.SelectedItem is null)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please select before moving"));
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var swapCommand = scope.ServiceProvider.GetRequiredService<SwapCommand.Handler>();
            var newIndex = await swapCommand.HandleAsync(new(new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Down));
            Jobs.SelectedIndex = newIndex;
            await JobsModifiedCommand.Execute(new JobsModified(VillageId));
        }

        [ReactiveCommand]
        private async Task Top()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            if (Jobs.SelectedItem is null)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please select before moving"));
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var moveCommand = scope.ServiceProvider.GetRequiredService<MoveCommand.Handler>();
            var newIndex = await moveCommand.HandleAsync(new(new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Top));
            Jobs.SelectedIndex = newIndex;

            await JobsModifiedCommand.Execute(new JobsModified(VillageId));
        }

        [ReactiveCommand]
        private async Task Bottom()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            if (Jobs.SelectedItem is null)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please select before moving"));
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var moveCommand = scope.ServiceProvider.GetRequiredService<MoveCommand.Handler>();
            var newIndex = await moveCommand.HandleAsync(new(new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Bottom));
            Jobs.SelectedIndex = newIndex;
            await JobsModifiedCommand.Execute(new JobsModified(VillageId));
        }

        [ReactiveCommand]
        private async Task Delete()
        {
            if (!IsAccountPaused(AccountId))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please pause account before modifing building queue"));
                return;
            }
            if (Jobs.SelectedItem is null) return;
            var jobId = Jobs.SelectedItem.Id;

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var deleteJobByIdCommand = scope.ServiceProvider.GetRequiredService<DeleteJobByIdCommand.Handler>();
            await deleteJobByIdCommand.HandleAsync(new(new JobId(jobId)));
            await JobsModifiedCommand.Execute(new JobsModified(VillageId));
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

            await JobsModifiedCommand.Execute(new JobsModified(VillageId));
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
                jobs = JsonSerializer.Deserialize<List<JobDto>>(jsonString)!;
            }
            catch
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Invalid file."));
                return;
            }

            var confirm = await _dialogService.ConfirmBox.Handle(new MessageBoxData("Warning", "TBS will remove resource field build job if its position doesn't match with current village."));
            if (!confirm) return;

            var shuffle = await _dialogService.ConfirmBox.Handle(new MessageBoxData("Warning", "Do you want to random building location?"));

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var fixJobsCommand = scope.ServiceProvider.GetRequiredService<FixJobsCommand.Handler>();
            var fixedJobs = await fixJobsCommand.HandleAsync(new(VillageId, jobs, shuffle));
            var importCommand = scope.ServiceProvider.GetRequiredService<ImportCommand.Handler>();
            await importCommand.HandleAsync(new(VillageId, fixedJobs));
            await JobsModifiedCommand.Execute(new JobsModified(VillageId));
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
            var status = _taskManager.GetStatus(accountId);
            if (status == StatusEnums.Online)
            {
                return false;
            }
            return true;
        }
    }
}