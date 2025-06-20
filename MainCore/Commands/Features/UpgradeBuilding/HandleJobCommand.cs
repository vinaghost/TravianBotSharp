using MainCore.Constraints;
using System.Text.Json;
using MainCore.Queries;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class HandleJobCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;
        public sealed record Response(NormalBuildPlan Plan, JobId JobId);

        private static async ValueTask<Result<Response>> HandleAsync(
            Command command,
            GetJobQuery.Handler getJobQuery,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery,
            DeleteJobByIdCommand.Handler deleteJobByIdCommand,
            JobUpdated.Handler jobUpdated,
            GetStorageQuery.Handler getStorageQuery,
            CancellationToken cancellationToken
        )
        {
            var (accountId, villageId) = command;

            var initialUpdateResult = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            if (initialUpdateResult.IsFailed) return Result.Fail<Response>(initialUpdateResult.Errors);

            var (_, isFailed, job, errors) = await getJobQuery.HandleAsync(new(accountId, villageId), cancellationToken);
            if (isFailed) return Result.Fail<Response>(errors);

            Result result;
            if (job.Type == JobTypeEnums.ResourceBuild)
            {
                var storage = await getStorageQuery.HandleAsync(new(villageId), cancellationToken);

                var layoutBuildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId, true));
                var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content)!;
                var normalBuildPlan = GetNormalBuildPlan(resourceBuildPlan, layoutBuildings, storage);
                if (normalBuildPlan is null)
                {
                    await deleteJobByIdCommand.HandleAsync(new(villageId, job.Id, "Normal build plan could not be resolved"), cancellationToken);
                    await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
                    return Result.Fail<Response>(Continue.Error);
                }

                var dorfIndex = normalBuildPlan.Location < 19 ? 1 : 2;
                result = await toDorfCommand.HandleAsync(new(accountId, dorfIndex), cancellationToken);
                if (result.IsFailed) return Result.Fail<Response>(result.Errors);

                var updateResult = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
                if (updateResult.IsFailed) return Result.Fail<Response>(updateResult.Errors);

                return Result.Ok(new Response(normalBuildPlan, job.Id));
            }

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;

            var dorf = plan.Location < 19 ? 1 : 2;
            result = await toDorfCommand.HandleAsync(new(accountId, dorf), cancellationToken);
            if (result.IsFailed) return Result.Fail<Response>(result.Errors);

            var updateBuildingCommandResult = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            if (updateBuildingCommandResult.IsFailed) return Result.Fail<Response>(updateBuildingCommandResult.Errors);

            var (buildings, queueBuildings) = updateBuildingCommandResult.Value;
            if (IsJobComplete(job, buildings, queueBuildings))
            {
                await deleteJobByIdCommand.HandleAsync(new(villageId, job.Id, "Job already completed"), cancellationToken);
                await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
                return Result.Fail<Response>(Continue.Error);
            }

            return Result.Ok(new Response(plan, job.Id));
        }

        private static NormalBuildPlan? GetNormalBuildPlan(
            ResourceBuildPlan plan,
            List<BuildingItem> layoutBuildings,
            StorageDto storage
        )
        {
            var fieldList = new Dictionary<ResourcePlanEnums, List<BuildingEnums>>()
            {
                {
                    ResourcePlanEnums.AllResources,
                    new()
                    {
                        BuildingEnums.Woodcutter,
                        BuildingEnums.ClayPit,
                        BuildingEnums.IronMine,
                        BuildingEnums.Cropland,
                    }
                },
                {
                    ResourcePlanEnums.ExcludeCrop,
                    new()
                    {
                        BuildingEnums.Woodcutter,
                        BuildingEnums.ClayPit,
                        BuildingEnums.IronMine,
                    }
                },
                { ResourcePlanEnums.OnlyCrop, new() { BuildingEnums.Cropland, } },
            };

            var fieldTypes = fieldList[plan.Plan];

            layoutBuildings = layoutBuildings
                .Where(x => fieldTypes.Contains(x.Type))
                .Where(x => x.Level < plan.Level)
                .ToList();

            if (layoutBuildings.Count == 0) return null;

            var typeStorage = new Dictionary<BuildingEnums, long>
            {
                { BuildingEnums.Woodcutter, storage.Wood },
                { BuildingEnums.ClayPit, storage.Clay },
                { BuildingEnums.IronMine, storage.Iron },
                { BuildingEnums.Cropland, storage.Crop },
            };

            var candidates = layoutBuildings
                .Select(x => new
                {
                    Building = x,
                    NextLevel = Math.Max(Math.Max(x.CurrentLevel, x.QueueLevel), x.JobLevel) + 1,
                })
                .ToList();

            if (candidates.Count == 0) return null;

            var upgradeable = candidates
                .Where(x => !storage.IsResourceEnough(x.Building.Type.GetCost(x.NextLevel)).IsFailed)
                .ToList();

            if (upgradeable.Count == 0)
            {
                var soonest = candidates
                    .Select(x => new
                    {
                        x.Building,
                        x.NextLevel,
                        Time = GetTimeUntilAffordable(storage, x.Building.Type.GetCost(x.NextLevel))
                    })
                    .ToList();

                var minTime = soonest.Min(x => x.Time);
                upgradeable = soonest
                    .Where(x => x.Time == minTime)
                    .Select(x => new { x.Building, x.NextLevel })
                    .ToList();
            }

            candidates = upgradeable.Count > 0 ? upgradeable : candidates;

            var minLevel = candidates
                .Select(x => x.Building.Level)
                .Min();

            candidates = candidates
                .Where(x => x.Building.Level == minLevel)
                .ToList();

            var minResource = candidates
                .Min(x => typeStorage[x.Building.Type]);

            var chosenOne = candidates
                .Where(x => typeStorage[x.Building.Type] == minResource)
                .OrderBy(x => x.Building.Location)
                .First();

            return new NormalBuildPlan
            {
                Type = chosenOne.Building.Type,
                Level = chosenOne.NextLevel,
                Location = chosenOne.Building.Location,
            };
        }

        private static double GetTimeUntilAffordable(StorageDto storage, long[] required)
        {
            var missing = storage.GetMissingResource(required);
            var times = new double[4];

            times[0] = storage.ProductionWood > 0 ? missing[0] / (double)storage.ProductionWood : double.PositiveInfinity;
            times[1] = storage.ProductionClay > 0 ? missing[1] / (double)storage.ProductionClay : double.PositiveInfinity;
            times[2] = storage.ProductionIron > 0 ? missing[2] / (double)storage.ProductionIron : double.PositiveInfinity;
            times[3] = storage.ProductionCrop > 0 ? missing[3] / (double)storage.ProductionCrop : double.PositiveInfinity;

            return times.Max();
        }

        private static bool IsJobComplete(JobDto job, List<BuildingDto> buildings, List<QueueBuilding> queueBuildings)
        {
            if (job.Type == JobTypeEnums.ResourceBuild) return false;

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;

            // Only consider queue/buildings with matching location AND type
            var maxQueueLevel = queueBuildings
                .Where(x => x.Location == plan.Location && x.Type == plan.Type)
                .Select(x => x.Level)
                .DefaultIfEmpty(int.MinValue)
                .Max();

            var maxCurrentLevel = buildings
                .Where(x => x.Location == plan.Location && x.Type == plan.Type)
                .Select(x => x.Level)
                .DefaultIfEmpty(int.MinValue)
                .Max();

            // If either the current or queued level is >= requested, job is complete
            if (maxQueueLevel >= plan.Level || maxCurrentLevel >= plan.Level) return true;

            return false;
        }
    }
}