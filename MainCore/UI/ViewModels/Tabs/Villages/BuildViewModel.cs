using Humanizer;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using MainCore.Infrasturecture.Extensions;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Extensions;

    [RegisterSingleton<BuildViewModel>]
    public partial class BuildViewModel : VillageTabViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;
        private readonly IValidator<NormalBuildInput> _normalBuildInputValidator;
        private readonly IValidator<ResourceBuildInput> _resourceBuildInputValidator;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IRxQueue _rxQueue;

        public NormalBuildInput NormalBuildInput { get; } = new();
        public ResourceBuildInput ResourceBuildInput { get; } = new();

        public ListBoxItemViewModel Buildings { get; } = new();
        public ListBoxItemViewModel Queue { get; } = new();
        public ListBoxItemViewModel Jobs { get; } = new();

        public BuildViewModel(IDialogService dialogService, IValidator<NormalBuildInput> normalBuildInputValidator, IValidator<ResourceBuildInput> resourceBuildInputValidator, ITaskManager taskManager, IRxQueue rxQueue, IDbContextFactory<AppDbContext> contextFactory)
        {
            _dialogService = dialogService;
            _normalBuildInputValidator = normalBuildInputValidator;
            _resourceBuildInputValidator = resourceBuildInputValidator;
            _taskManager = taskManager;
            _rxQueue = rxQueue;
            _contextFactory = contextFactory;

            Init();
        }

        private void Init()
        {
            this.WhenAnyValue(vm => vm.Buildings.SelectedItem)
                .ObserveOn(RxSchedulers.TaskpoolScheduler)
                .WhereNotNull()
                .InvokeCommand(LoadBuildNormalCommand);

            LoadBuildingCommand.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(Buildings.Load);
            LoadJobCommand.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(Jobs.Load);
            LoadQueueCommand.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(Queue.Load);

            LoadBuildNormalCommand.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(SetNormalBuildInput);

            BuildNormalCommand.InvokeCommand(JobsModifiedCommand);
            BuildResourceCommand.InvokeCommand(JobsModifiedCommand);
            UpgradeOneLevelCommand.InvokeCommand(JobsModifiedCommand);
            UpgradeMaxLevelCommand.InvokeCommand(JobsModifiedCommand);

            UpCommand.InvokeCommand(JobsModifiedCommand);
            DownCommand.InvokeCommand(JobsModifiedCommand);
            TopCommand.InvokeCommand(JobsModifiedCommand);
            BottomCommand.InvokeCommand(JobsModifiedCommand);

            DeleteCommand.InvokeCommand(JobsModifiedCommand);
            DeleteAllCommand.InvokeCommand(JobsModifiedCommand);

            ImportCommand.InvokeCommand(JobsModifiedCommand);

            _rxQueue.RegisterCommand(BuildingsModifiedCommand);
            _rxQueue.RegisterCommand(JobsModifiedCommand);

            void SetNormalBuildInput(List<BuildingEnums> buildings)
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
            }
        }

        [ReactiveCommand(RunInBackground = true)]
        public async Task BuildingsModified(BuildingsModified notification)
        {
            using var context = _contextFactory.CreateDbContext();
            var task = new CompleteImmediatelyTask.Task(AccountId, notification.VillageId);
            if (task.CanStart(context) && !_taskManager.IsExist<CompleteImmediatelyTask.Task>(AccountId, notification.VillageId))
            {
                _taskManager.Add(task);
            }

            if (!IsActive) return;
            if (notification.VillageId != VillageId) return;

            await LoadQueueCommand.Execute(notification.VillageId).ToHotTask();
            await LoadBuildingCommand.Execute(notification.VillageId).ToHotTask();
        }

        [ReactiveCommand(RunInBackground = true)]
        public async Task JobsModified(JobsModified notification)
        {
            _taskManager.AddOrUpdate(new UpgradeBuildingTask.Task(AccountId, notification.VillageId));

            if (!IsActive) return;
            if (notification.VillageId != VillageId) return;

            await LoadJobCommand.Execute(notification.VillageId).ToHotTask();
            await LoadBuildingCommand.Execute(notification.VillageId).ToHotTask();
        }

        protected override async Task Load(VillageId villageId)
        {
            await LoadJobCommand.Execute(villageId).ToHotTask();
            await LoadBuildingCommand.Execute(villageId).ToHotTask();
            await LoadQueueCommand.Execute(villageId).ToHotTask();
        }

        [ReactiveCommand(RunInBackground = true)]
        private List<ListBoxItem> LoadBuilding(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var buildings = context.GetLayoutBuildings(villageId);

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

        [ReactiveCommand(RunInBackground = true)]
        private List<ListBoxItem> LoadQueue(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
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

        [ReactiveCommand(RunInBackground = true)]
        private List<ListBoxItem> LoadJob(VillageId villageId)
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

        private static readonly List<BuildingEnums> AvailableBuildings =
        [
            .. Enum.GetValues<BuildingEnums>().Where(x => !IgnoreBuildings.Contains(x))
        ];

        [ReactiveCommand(RunInBackground = true)]
        private List<BuildingEnums> LoadBuildNormal(ListBoxItem item)
        {
            if (item is null) return [];

            using var context = _contextFactory.CreateDbContext();
            var buildingItems = context.GetLayoutBuildings(VillageId);

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

            return [.. AvailableBuildings.Where(x => !buildings.Contains(x))];
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task BuildNormal()
        {
            if (!await EnsureAccountPaused()) return;

            var result = await _normalBuildInputValidator.ValidateAsync(NormalBuildInput);
            if (!result.IsValid)
            {
                await _dialogService.SendMessage("Error", result.ToString());
                return;
            }

            if (!await EnsureBuildingSelected()) return;

            var location = Buildings.SelectedIndex + 1;

            var (type, level) = NormalBuildInput.Get();
            var plan = new NormalBuildPlan()
            {
                Location = location,
                Type = type,
                Level = level,
            };

            using var context = _contextFactory.CreateDbContext();
            var buildings = context.GetLayoutBuildings(VillageId);
            var building = buildings.Find(x => x.Location == plan.Location);

            if (building is null)
            {
                var checkResult = plan.Type.CheckRequirements(buildings);
                if (!checkResult.IsFailed)
                {
                    await _dialogService.SendMessage("Error", checkResult.ToString());
                    return;
                }
                plan.FixLocation(buildings);
            }

            context.AddJob(VillageId, plan);
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task UpgradeOneLevel()
        {
            if (!await EnsureAccountPaused()) return;
            if (!await EnsureBuildingSelected()) return;
            var location = Buildings.SelectedIndex + 1;

            using var context = _contextFactory.CreateDbContext();
            context.Upgrade(VillageId, location, false);
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task UpgradeMaxLevel()
        {
            if (!await EnsureAccountPaused()) return;
            if (!await EnsureBuildingSelected()) return;
            var location = Buildings.SelectedIndex + 1;

            using var context = _contextFactory.CreateDbContext();
            context.Upgrade(VillageId, location, true);
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task BuildResource()
        {
            if (!await EnsureAccountPaused()) return;

            var result = await _resourceBuildInputValidator.ValidateAsync(ResourceBuildInput);
            if (!result.IsValid)
            {
                await _dialogService.SendMessage("Error", result.ToString());
                return;
            }

            using var context = _contextFactory.CreateDbContext();
            context.AddJob(VillageId, ResourceBuildInput);
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Up()
        {
            if (!await EnsureAccountPaused()) return;
            if (!await EnsureJobSelected()) return;

            using var context = _contextFactory.CreateDbContext();
            var newIndex = context.SwapJob(new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Up);
            Jobs.SelectedIndex = newIndex;
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Down()
        {
            if (!await EnsureAccountPaused()) return;
            if (!await EnsureJobSelected()) return;

            using var context = _contextFactory.CreateDbContext();
            var newIndex = context.SwapJob(new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Down);
            Jobs.SelectedIndex = newIndex;
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Top()
        {
            if (!await EnsureAccountPaused()) return;
            if (!await EnsureJobSelected()) return;

            using var context = _contextFactory.CreateDbContext();
            var newIndex = context.MoveJob(new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Top);
            Jobs.SelectedIndex = newIndex;
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Bottom()
        {
            if (!await EnsureAccountPaused()) return;
            if (!await EnsureJobSelected()) return;

            using var context = _contextFactory.CreateDbContext();
            var newIndex = context.MoveJob(new JobId(Jobs[Jobs.SelectedIndex].Id), MoveEnums.Bottom);
            Jobs.SelectedIndex = newIndex;
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Delete()
        {
            if (!await EnsureAccountPaused()) return;
            if (Jobs.SelectedItem is null) return;
            var jobId = Jobs.SelectedItem.Id;

            using var context = _contextFactory.CreateDbContext();
            context.DeleteJobById(new JobId(jobId));
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task DeleteAll()
        {
            if (!await EnsureAccountPaused()) return;

            using var context = _contextFactory.CreateDbContext();
            context.Jobs
                .Where(x => x.VillageId == VillageId.Value)
                .ExecuteDelete();
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Import()
        {
            if (!await EnsureAccountPaused()) return;
            var path = await _dialogService.OpenFileDialog();
            if (string.IsNullOrEmpty(path)) return;
            List<JobDto> jobs;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path);
                jobs = JsonSerializer.Deserialize<List<JobDto>>(jsonString) ?? [];
            }
            catch
            {
                await _dialogService.SendMessage("Warning", "Invalid file.");
                return;
            }

            if (jobs.Count == 0)
            {
                await _dialogService.SendMessage("Warning", "No jobs found in file.");
                return;
            }

            var confirm = await _dialogService.SendConfirm("Warning", "TBS will remove resource field build job if its position doesn't match with current village.");
            if (!confirm) return;

            var shuffle = await _dialogService.SendConfirm("Warning", "Do you want to random building location?");

            using var context = _contextFactory.CreateDbContext();

            var fixedJobs = context.FixJobs(VillageId, jobs, shuffle);
            var count = context.Jobs
                .Count(x => x.VillageId == VillageId.Value);

            var additionJobs = fixedJobs
                .Select((job, index) => new Job()
                {
                    Position = count + index,
                    VillageId = VillageId.Value,
                    Type = job.Type,
                    Content = job.Content,
                })
                .ToList();

            context.AddRange(additionJobs);
            context.SaveChanges();
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Export()
        {
            if (!await EnsureAccountPaused()) return;

            var path = await _dialogService.SaveFileDialog();
            if (string.IsNullOrEmpty(path)) return;

            using var context = _contextFactory.CreateDbContext();
            var jobs = context.Jobs
                .Where(x => x.VillageId == VillageId.Value)
                .OrderBy(x => x.Position)
                .ToDto()
                .ToList();
            jobs.ForEach(job => job.Id = JobId.Empty);
            var jsonString = JsonSerializer.Serialize(jobs);
            await File.WriteAllTextAsync(path, jsonString);

            await _dialogService.SendMessage("Information", "Job list exported");
        }

        private async Task<bool> EnsureAccountPaused()
        {
            if (IsAccountPaused(AccountId)) return true;
            await _dialogService.SendMessage("Warning", "Please pause account before modifying building queue");
            return false;
        }

        private async Task<bool> EnsureJobSelected()
        {
            if (Jobs.SelectedItem is not null) return true;
            await _dialogService.SendMessage("Warning", "Please select before moving");
            return false;
        }

        private async Task<bool> EnsureBuildingSelected()
        {
            if (Buildings.SelectedItem is not null) return true;
            await _dialogService.SendMessage("Warning", "Please select building before adding job");
            return false;
        }

        private bool IsAccountPaused(AccountId accountId)
        {
            var status = _taskManager.GetStatus(accountId);
            return status != StatusEnums.Online;
        }
    }
}