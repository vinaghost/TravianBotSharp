using MainCore.Constraints;
using System.Text.Json;

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
            AddJobCommand.Handler addJobCommand,
            JobUpdated.Handler jobUpdated,
            CancellationToken cancellationToken
        )
        {
            var (accountId, villageId) = command;

            var (_, isFailed, job, errors) = await getJobQuery.HandleAsync(new(accountId, villageId), cancellationToken);
            if (isFailed) return Result.Fail<Response>(errors);

            Result result;
            if (job.Type == JobTypeEnums.ResourceBuild)
            {
                var layoutBuildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId, true));
                var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content)!;
                var normalBuildPlan = GetNormalBuildPlan(villageId, resourceBuildPlan, layoutBuildings);
                if (normalBuildPlan is null)
                {
                    await deleteJobByIdCommand.HandleAsync(new(villageId, job.Id), cancellationToken);
                }
                else
                {
                    await addJobCommand.HandleAsync(new(villageId, normalBuildPlan.ToJob(), true));
                }
                await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
                return Result.Fail<Response>(Continue.Error);
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
            VillageId villageId,
            ResourceBuildPlan plan,
            List<BuildingItem> layoutBuildings
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

            var minLevel = layoutBuildings
                .Select(x => x.Level)
                .Min();

            var chosenOne = layoutBuildings
                .Where(x => x.Level == minLevel)
                .OrderBy(x => x.Id.Value + Random.Shared.Next())
                .FirstOrDefault();

            if (chosenOne is null) return null;

            // Fix: Use the highest of current, queue, and job levels for the next job
            int nextLevel = Math.Max(Math.Max(chosenOne.CurrentLevel, chosenOne.QueueLevel), chosenOne.JobLevel) + 1;

            var normalBuildPlan = new NormalBuildPlan()
            {
                Type = chosenOne.Type,
                Level = nextLevel,
                Location = chosenOne.Location,
            };
            return normalBuildPlan;
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