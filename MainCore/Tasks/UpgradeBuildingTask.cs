using MainCore.Commands.Features.UpgradeBuilding;
using MainCore.Commands.Features.UseHeroItem;
using MainCore.Common.Errors.AutoBuilder;
using MainCore.Common.Errors.Storage;
using MainCore.Common.Models;
using MainCore.Tasks.Base;
using System.Text.Json;

namespace MainCore.Tasks
{
    [RegisterAsTask]
    public class UpgradeBuildingTask : VillageTask
    {
        private readonly ILogService _logService;
        private readonly ITaskManager _taskManager;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public UpgradeBuildingTask(ILogService logService, ITaskManager taskManager, IDbContextFactory<AppDbContext> contextFactory)
        {
            _logService = logService;
            _taskManager = taskManager;
            _contextFactory = contextFactory;
        }

        protected override async Task<Result> Execute()
        {
            var logger = _logService.GetLogger(AccountId);
            Result result;
            while (true)
            {
                if (CancellationToken.IsCancellationRequested) return Cancel.Error;

                result = await new ToDorfCommand().Execute(_chromeBrowser, 0, false, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await new UpdateBuildingCommand().Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);

                var jobResult = await new GetJobCommand().Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);

                if (jobResult.IsFailed)
                {
                    if (jobResult.HasError<BuildingQueue>())
                    {
                        await SetTimeQueueBuildingComplete();
                        return Skip.BuildingQueueFull;
                    }

                    return Result.Fail(jobResult.Errors)
                        .WithError(TraceMessage.Error(TraceMessage.Line()));
                }

                var job = jobResult.Value;

                if (job.Type == JobTypeEnums.ResourceBuild)
                {
                    result = await new ExtractResourceFieldJobCommand().Execute(AccountId, VillageId, job, CancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    continue;
                }

                var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

                var dorf = plan.Location < 19 ? 1 : 2;
                result = await new ToDorfCommand().Execute(_chromeBrowser, dorf, false, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await new UpdateBuildingCommand().Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);

                if (await JobComplete(job))
                {
                    continue;
                }

                logger.Information("Build {Type} to level {Level} at location {Location}", plan.Type, plan.Level, plan.Location);

                result = await ToBuildingPage(_chromeBrowser, plan);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await new UpdateStorageCommand().Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);

                var requiredResource = GetRequiredResource(_chromeBrowser, plan);

                result = IsEnoughResource(VillageId, requiredResource);
                if (result.IsFailed)
                {
                    if (result.HasError<FreeCrop>())
                    {
                        await AddCropland();
                        continue;
                    }

                    if (result.HasError<GranaryLimit>() || result.HasError<WarehouseLimit>())
                    {
                        return result
                            .WithError(Stop.NotEnoughStorageCapacity)
                            .WithError(TraceMessage.Error(TraceMessage.Line()));
                    }

                    if (IsUseHeroResource())
                    {
                        var missingResource = GetMissingResource(VillageId, requiredResource);
                        var heroResourceResult = await new UseHeroResourceCommand().Execute(AccountId, _chromeBrowser, missingResource, CancellationToken);
                        if (heroResourceResult.IsFailed)
                        {
                            if (!heroResourceResult.HasError<Retry>())
                            {
                                await SetEnoughResourcesTime(_chromeBrowser, plan);
                            }

                            return heroResourceResult.WithError(TraceMessage.Error(TraceMessage.Line()));
                        }
                    }
                    else
                    {
                        await SetEnoughResourcesTime(_chromeBrowser, plan);
                        return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    }
                }
                if (IsUpgradeable(plan))
                {
                    if (IsSpecialUpgrade() && IsSpecialUpgradeable(plan))
                    {
                        result = await new SpecialUpgradeCommand().Execute(_chromeBrowser, AccountId, CancellationToken);
                        if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    }
                    else
                    {
                        result = await new UpgradeCommand().Execute(_chromeBrowser, CancellationToken);
                        if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                    }
                }
                else
                {
                    result = await new ConstructCommand().Execute(_chromeBrowser, plan.Type, CancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                }
            }
        }

        protected override void SetName()
        {
            var village = new GetVillageName().Execute(VillageId);
            _name = $"Upgrade building in {village}";
        }

        private bool IsUpgradeable(NormalBuildPlan plan)
        {
            return !IsEmptySite(VillageId, plan.Location);
        }

        private bool IsSpecialUpgradeable(NormalBuildPlan plan)
        {
            var buildings = new List<BuildingEnums>()
            {
                BuildingEnums.Residence,
                BuildingEnums.Palace,
                BuildingEnums.CommandCenter
            };

            if (buildings.Contains(plan.Type)) return false;

            if (plan.Type.IsResourceField())
            {
                var dto = GetBuilding(VillageId, plan.Location);
                if (dto.Level == 0) return false;
            }
            return true;
        }

        private bool IsSpecialUpgrade()
        {
            var useSpecialUpgrade = new GetSetting().BooleanByName(VillageId, VillageSettingEnums.UseSpecialUpgrade);
            return useSpecialUpgrade;
        }

        private bool IsUseHeroResource()
        {
            var useHeroResource = new GetSetting().BooleanByName(VillageId, VillageSettingEnums.UseHeroResourceForBuilding);
            return useHeroResource;
        }

        private async Task SetEnoughResourcesTime(IChromeBrowser _chromeBrowser, NormalBuildPlan plan)
        {
            var time = GetTimeWhenEnoughResource(_chromeBrowser, plan);
            ExecuteAt = DateTime.Now.Add(time);
            await _taskManager.ReOrder(AccountId);
        }

        private async Task SetTimeQueueBuildingComplete()
        {
            var buildingQueue = GetFirstQueueBuilding(VillageId);
            if (buildingQueue is null)
            {
                ExecuteAt = DateTime.Now.AddSeconds(1);
                await _taskManager.ReOrder(AccountId);
                return;
            }

            ExecuteAt = buildingQueue.CompleteTime.AddSeconds(3);
            await _taskManager.ReOrder(AccountId);
        }

        public async Task AddCropland()
        {
            var cropland = GetCropland(VillageId);

            var plan = new NormalBuildPlan()
            {
                Location = cropland.Location,
                Type = cropland.Type,
                Level = cropland.Level + 1,
            };

            new AddJobCommand().ToTop(VillageId, plan);
            await _mediator.Publish(new JobUpdated(AccountId, VillageId));
        }

        public async Task<Result> ToBuildingPage(IChromeBrowser _chromeBrowser, NormalBuildPlan plan)
        {
            Result result;
            result = await new ToBuildingCommand().Execute(_chromeBrowser, plan.Location, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var building = GetBuilding(VillageId, plan.Location);
            if (building.Type == BuildingEnums.Site)
            {
                var tabIndex = plan.Type.GetBuildingsCategory();
                result = await new SwitchTabCommand().Execute(_chromeBrowser, tabIndex, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else
            {
                if (building.Level <= 0) return Result.Ok();
                if (!building.Type.HasMultipleTabs()) return Result.Ok();
                result = await new SwitchTabCommand().Execute(_chromeBrowser, 0, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            return Result.Ok();
        }

        public long[] GetRequiredResource(IChromeBrowser _chromeBrowser, NormalBuildPlan plan)
        {
            var html = _chromeBrowser.Html;

            var isEmptySite = IsEmptySite(VillageId, plan.Location);
            return GetRequiredResource(html, isEmptySite, plan.Type);
        }

        public TimeSpan GetTimeWhenEnoughResource(IChromeBrowser _chromeBrowser, NormalBuildPlan plan)
        {
            var html = _chromeBrowser.Html;
            var isEmptySite = IsEmptySite(VillageId, plan.Location);
            return GetTimeWhenEnoughResource(html, isEmptySite, plan.Type);
        }

        private static TimeSpan GetTimeWhenEnoughResource(HtmlDocument doc, bool isEmptySite, BuildingEnums building = BuildingEnums.Site)
        {
            HtmlNode node;
            if (isEmptySite)
            {
                node = doc.GetElementbyId($"contract_building{(int)building}");
            }
            else
            {
                node = doc.GetElementbyId("contract");
            }
            if (node is null) return TimeSpan.Zero;

            var errorMessage = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("errorMessage"));
            if (errorMessage is null) return TimeSpan.Zero;
            var timer = errorMessage.Descendants("span")
                .FirstOrDefault(x => x.HasClass("timer"));
            if (timer is null) return TimeSpan.Zero;
            var time = timer.GetAttributeValue("value", 0);
            return TimeSpan.FromSeconds(time);
        }

        private static long[] GetRequiredResource(HtmlDocument doc, bool isEmptySite, BuildingEnums building = BuildingEnums.Site)
        {
            HtmlNode node;
            if (isEmptySite)
            {
                node = doc.GetElementbyId($"contract_building{(int)building}");
            }
            else
            {
                node = doc.GetElementbyId("contract");
            }

            if (node is null) return Enumerable.Repeat(-1L, 5).ToArray();
            var resourceWrapper = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("resourceWrapper"));
            if (resourceWrapper is null) return Enumerable.Repeat(-1L, 5).ToArray();

            var resources = resourceWrapper.Descendants("div")
                .Where(x => x.HasClass("resource"))
                .ToList();

            if (resources.Count != 5) return Enumerable.Repeat(-1L, 5).ToArray();

            var resourceBuilding = new long[5];
            for (var i = 0; i < 5; i++)
            {
                resourceBuilding[i] = resources[i].InnerText.ParseLong();
            }

            return resourceBuilding;
        }

        private Building GetCropland(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var building = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == BuildingEnums.Cropland)
                .OrderBy(x => x.Level)
                .FirstOrDefault();
            return building;
        }

        private BuildingDto GetBuilding(VillageId villageId, int location)
        {
            using var context = _contextFactory.CreateDbContext();
            var building = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == location)
                .ToDto()
                .FirstOrDefault();
            return building;
        }

        private bool IsEmptySite(VillageId villageId, int location)
        {
            using var context = _contextFactory.CreateDbContext();
            bool isEmptySite = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == location)
                .Where(x => x.Type == BuildingEnums.Site || x.Level == -1)
                .Any();

            return isEmptySite;
        }

        private long[] GetMissingResource(VillageId villageId, long[] requiredResource)
        {
            using var context = _contextFactory.CreateDbContext();
            var storage = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault();
            if (storage is null) return new long[4] { 0, 0, 0, 0 };

            var resource = new long[4];
            if (storage.Wood < requiredResource[0]) resource[0] = requiredResource[0] - storage.Wood;
            if (storage.Clay < requiredResource[1]) resource[1] = requiredResource[1] - storage.Clay;
            if (storage.Iron < requiredResource[2]) resource[2] = requiredResource[2] - storage.Iron;
            if (storage.Crop < requiredResource[3]) resource[3] = requiredResource[3] - storage.Crop;
            return resource;
        }

        private Result IsEnoughResource(VillageId villageId, long[] requiredResource)
        {
            using var context = _contextFactory.CreateDbContext();
            var storage = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault();

            if (storage is null) return Result.Ok();

            var result = Result.Ok();
            if (storage.Wood < requiredResource[0]) result.WithError(Resource.Error("wood", storage.Wood, requiredResource[0]));
            if (storage.Clay < requiredResource[1]) result.WithError(Resource.Error("clay", storage.Clay, requiredResource[1]));
            if (storage.Iron < requiredResource[2]) result.WithError(Resource.Error("iron", storage.Iron, requiredResource[2]));
            if (storage.Crop < requiredResource[3]) result.WithError(Resource.Error("crop", storage.Wood, requiredResource[3]));
            if (storage.FreeCrop < requiredResource[4]) result.WithError(FreeCrop.Error(storage.Wood, requiredResource[4]));

            var max = requiredResource.Max();
            if (storage.Warehouse < max) result.WithError(WarehouseLimit.Error(storage.Warehouse, max));
            if (storage.Granary < requiredResource[3]) result.WithError(GranaryLimit.Error(storage.Granary, requiredResource[3]));
            return result;
        }

        private QueueBuilding GetFirstQueueBuilding(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .OrderBy(x => x.Position)
                .FirstOrDefault();
            return queueBuilding;
        }

        private async Task<bool> JobComplete(JobDto job)
        {
            if (JobComplete(VillageId, job))
            {
                new DeleteJobCommand().ByJobId(job.Id);
                await _mediator.Publish(new JobUpdated(AccountId, VillageId));
                return true;
            }
            return false;
        }

        private bool JobComplete(VillageId villageId, JobDto job)
        {
            if (job.Type == JobTypeEnums.ResourceBuild) return false;
            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

            using var context = _contextFactory.CreateDbContext();

            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == plan.Location)
                .OrderByDescending(x => x.Level)
                .Select(x => x.Level)
                .FirstOrDefault();

            if (queueBuilding >= plan.Level) return true;

            var villageBuilding = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == plan.Location)
                .Select(x => x.Level)
                .FirstOrDefault();
            if (villageBuilding >= plan.Level) return true;

            return false;
        }
    }
}