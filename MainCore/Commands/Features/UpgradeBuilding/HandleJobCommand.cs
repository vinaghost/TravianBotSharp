using MainCore.Constraints;
using System.Text.Json;
using MainCore.Infrasturecture.Persistence;

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
            AppDbContext context,
            CancellationToken cancellationToken
        )
        {
            var (accountId, villageId) = command;

            var (_, isFailed, job, errors) = await getJobQuery.HandleAsync(new(accountId, villageId), cancellationToken);
            if (isFailed) return Result.Fail<Response>(errors);

            Result result;
            if (job.Type == JobTypeEnums.ResourceBuild)
            {
                var storage = context.Storages
                    .Where(x => x.VillageId == villageId.Value)
                    .Select(x => new StorageDto
                    {
                        Wood = x.Wood,
                        Clay = x.Clay,
                        Iron = x.Iron,
                        Crop = x.Crop
                    })
                    .FirstOrDefault() ?? new();

                var layoutBuildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId, true));
                var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content)!;
                var normalBuildPlan = GetNormalBuildPlan(resourceBuildPlan, layoutBuildings, storage);
                if (normalBuildPlan is null)
                {
                    await deleteJobByIdCommand.HandleAsync(new(villageId, job.Id), cancellationToken);
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
                await deleteJobByIdCommand.HandleAsync(new(villageId, job.Id), cancellationToken);
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

            var orderedTypes = fieldTypes
                .OrderBy(t => typeStorage[t])
                .ToList();

            foreach (var type in orderedTypes)
            {
                var candidates = layoutBuildings
                    .Where(x => x.Type == type)
                    .ToList();
                if (candidates.Count == 0) continue;

                var minLevel = candidates
                    .Select(x => x.Level)
                    .Min();

                var chosenOne = candidates
                    .Where(x => x.Level == minLevel)
                    .OrderBy(x => x.Id.Value + Random.Shared.Next())
                    .FirstOrDefault();

                if (chosenOne is null) continue;

                int nextLevel = Math.Max(Math.Max(chosenOne.CurrentLevel, chosenOne.QueueLevel), chosenOne.JobLevel) + 1;

                return new NormalBuildPlan
                {
                    Type = chosenOne.Type,
                    Level = nextLevel,
                    Location = chosenOne.Location,
                };
            }

            return null;
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