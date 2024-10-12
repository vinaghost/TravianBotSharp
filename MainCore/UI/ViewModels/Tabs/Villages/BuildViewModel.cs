using FluentValidation;
using Humanizer;
using MainCore.Commands.UI.Build;
using MainCore.Common.Models;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton(Registration = RegistrationStrategy.Self)]
    public class BuildViewModel : VillageTabViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;
        private readonly IValidator<ResourceBuildInput> _resourceBuildInputValidator;

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

        public BuildViewModel(IMediator mediator, IDbContextFactory<AppDbContext> contextFactory, IDialogService dialogService, ITaskManager taskManager, IValidator<ResourceBuildInput> resourceBuildInputValidator)
        {
            _mediator = mediator;
            _contextFactory = contextFactory;
            _dialogService = dialogService;
            _taskManager = taskManager;
            _resourceBuildInputValidator = resourceBuildInputValidator;

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

            LoadBuilding.Subscribe(buildings => Buildings.Load(buildings));
            LoadJob.Subscribe(jobs => Jobs.Load(jobs));
            LoadQueue.Subscribe(queue => Queue.Load(queue));
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
            var buildings = GetBuildingItems(villageId);
            return buildings;
        }

        private List<ListBoxItem> LoadQueueHandler(VillageId villageId)
        {
            var queue = GetQueueItems(villageId);
            return queue;
        }

        private List<ListBoxItem> LoadJobHandler(VillageId villageId)
        {
            var jobs = GetJobItems(villageId);
            return jobs;
        }

        private List<BuildingEnums> LoadBuildNormalHanlder(ListBoxItem item)
        {
            if (item is null) return new();
            var buildings = GetNormalBuilding(VillageId, new BuildingId(item.Id));
            return buildings;
        }

        private async Task BuildNormalHandler()
        {
            var location = Buildings.SelectedIndex + 1;
            await new BuildNormalCommand().Execute(AccountId, VillageId, NormalBuildInput, location);
        }

        private async Task UpgradeOneLevelHandler()
        {
            var location = Buildings.SelectedIndex + 1;
            await UpgradeLevel(AccountId, VillageId, location, false);
        }

        private async Task UpgradeMaxLevelHandler()
        {
            var location = Buildings.SelectedIndex + 1;
            await UpgradeLevel(AccountId, VillageId, location, true);
        }

        private async Task ResourceNormalHandler()
        {
            var status = _taskManager.GetStatus(AccountId);
            if (status == StatusEnums.Online)
            {
                _dialogService.ShowMessageBox("Warning", "Please pause account before modifing building queue");
                return;
            }
            var result = await _resourceBuildInputValidator.ValidateAsync(ResourceBuildInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            var (type, level) = ResourceBuildInput.Get();
            var plan = new ResourceBuildPlan()
            {
                Plan = type,
                Level = level,
            };
            new AddJobCommand().ToBottom(VillageId, plan);
            await _mediator.Publish(new JobUpdated(AccountId, VillageId));
        }

        private async Task UpHandler()
        {
            if (Jobs.SelectedItem == null) return;
            await new MoveJobCommand().Execute(AccountId, VillageId, Jobs, MoveEnums.Up);
        }

        private async Task DownHandler()
        {
            if (Jobs.SelectedItem == null) return;
            await new MoveJobCommand().Execute(AccountId, VillageId, Jobs, MoveEnums.Down);
        }

        private async Task TopHandler()
        {
            if (Jobs.SelectedItem == null) return;
            await new MoveJobCommand().Execute(AccountId, VillageId, Jobs, MoveEnums.Top);
        }

        private async Task BottomHandler()
        {
            if (Jobs.SelectedItem == null) return;
            await new MoveJobCommand().Execute(AccountId, VillageId, Jobs, MoveEnums.Bottom);
        }

        private async Task DeleteHandler()
        {
            var status = _taskManager.GetStatus(AccountId);
            if (status == StatusEnums.Online)
            {
                _dialogService.ShowMessageBox("Warning", "Please pause account before modifying building queue");
                return;
            }
            if (Jobs.SelectedItem == null) return;
            var jobId = Jobs.SelectedItemId;

            new DeleteJobCommand().ByJobId(new JobId(jobId));
            await _mediator.Publish(new JobUpdated(AccountId, VillageId));
        }

        private async Task DeleteAllHandler()
        {
            var status = _taskManager.GetStatus(AccountId);
            if (status == StatusEnums.Online)
            {
                _dialogService.ShowMessageBox("Warning", "Please pause account before modifing building queue");
                return;
            }
            using var context = await _contextFactory.CreateDbContextAsync();

            //sqlite async dont work
#pragma warning disable S6966 // Awaitable method should be used
            context.Jobs
                .Where(x => x.VillageId == VillageId.Value)
                .ExecuteDelete();
#pragma warning restore S6966 // Awaitable method should be used

            await _mediator.Publish(new JobUpdated(AccountId, VillageId));
        }

        private async Task ImportHandler()
        {
            await new ImportCommand().Execute(AccountId, VillageId);
        }

        private async Task ExportHandler()
        {
            var path = _dialogService.SaveFileDialog();
            if (string.IsNullOrEmpty(path)) return;
            var jobs = GetJobs(VillageId);
            jobs.ForEach(job => job.Id = JobId.Empty);
            var jsonString = JsonSerializer.Serialize(jobs);
            await File.WriteAllTextAsync(path, jsonString);
            _dialogService.ShowMessageBox("Information", "Job list exported");
        }

        private static List<ListBoxItem> GetBuildingItems(VillageId villageId)
        {
            var items = new GetBuildings().Execute(villageId).Select(x => ToListBoxItem(x)).ToList();
            return items;
        }

        private static ListBoxItem ToListBoxItem(BuildingItem building)
        {
            const string arrow = " -> ";
            var sb = new StringBuilder();
            sb.Append(building.CurrentLevel);
            if (building.QueueLevel != 0)
            {
                var content = $"{arrow}({building.QueueLevel})";
                sb.Append(content);
            }
            if (building.JobLevel != 0)
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

        private List<BuildingEnums> GetNormalBuilding(VillageId villageId, BuildingId buildingId)
        {
            var buildingItems = new GetBuildings().Execute(villageId);
            var type = buildingItems
                .Where(x => x.Id == buildingId)
                .Select(x => x.Type)
                .FirstOrDefault();
            if (type != BuildingEnums.Site) return new() { type };
            using var context = _contextFactory.CreateDbContext();

            var buildings = buildingItems
                .Select(x => x.Type)
                .Where(x => !MultipleBuildings.Contains(x))
                .Distinct()
                .ToList();

            return AvailableBuildings.Where(x => !buildings.Contains(x)).ToList();
        }

        private static readonly List<BuildingEnums> MultipleBuildings = new()
        {
            BuildingEnums.Warehouse,
            BuildingEnums.Granary,
            BuildingEnums.Trapper,
            BuildingEnums.Cranny,
        };

        private static readonly List<BuildingEnums> IgnoreBuildings = new()
        {
            BuildingEnums.Site,
            BuildingEnums.Blacksmith,
            BuildingEnums.CityWall,
            BuildingEnums.EarthWall,
            BuildingEnums.Palisade,
            BuildingEnums.WW,
            BuildingEnums.StoneWall,
            BuildingEnums.MakeshiftWall,
        };

        private static readonly IEnumerable<BuildingEnums> AvailableBuildings = Enum.GetValues(typeof(BuildingEnums))
            .Cast<BuildingEnums>()
            .Where(x => !IgnoreBuildings.Contains(x));

        private List<ListBoxItem> GetQueueItems(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            var queue = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
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
            queue.AddRange(Enumerable.Range(0, count - queue.Count).Select(x => new ListBoxItem()));

            return queue;
        }

        private List<ListBoxItem> GetJobItems(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            var items = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .OrderBy(x => x.Position)
                .ToDto()
                .AsEnumerable()
                .Select(x => new ListBoxItem()
                {
                    Id = x.Id.Value,
                    Content = GetContent(x),
                })
                .ToList();

            return items;
        }

        private static string GetContent(JobDto job)
        {
            switch (job.Type)
            {
                case JobTypeEnums.NormalBuild:
                    {
                        var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);
                        return $"Build {plan.Type.Humanize()} to level {plan.Level} at location {plan.Location}";
                    }
                case JobTypeEnums.ResourceBuild:
                    {
                        var plan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content);
                        return $"Build {plan.Plan.Humanize()} to level {plan.Level}";
                    }
                default:
                    return job.Content;
            }
        }

        private List<JobDto> GetJobs(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            var jobs = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .OrderBy(x => x.Position)
                .ToDto()
                .ToList();
            return jobs;
        }

        public async Task UpgradeLevel(AccountId accountId, VillageId villageId, int location, bool isMaxLevel)
        {
            var status = _taskManager.GetStatus(accountId);
            if (status == StatusEnums.Online)
            {
                _dialogService.ShowMessageBox("Warning", "Please pause account before modifing building queue");
                return;
            }

            var buildings = new GetBuildings().Execute(villageId);
            var building = buildings.Find(x => x.Location == location);

            if (building is null) return;
            if (building.Type == BuildingEnums.Site) return;

            var level = 0;

            if (isMaxLevel)
            {
                level = building.Type.GetMaxLevel();
            }
            else
            {
                level = building.Level + 1;
            }

            var plan = new NormalBuildPlan()
            {
                Location = location,
                Type = building.Type,
                Level = level,
            };

            new AddJobCommand().ToBottom(villageId, plan);
            await _mediator.Publish(new JobUpdated(accountId, villageId));
        }
    }
}