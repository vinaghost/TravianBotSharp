using MainCore.Constraints;
using MainCore.Errors.AutoBuilder;
using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class HandleJobCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageQuery;

        private static async ValueTask<Result<NormalBuildPlan>> HandleAsync(
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
            if (isFailed) return Result.Fail(errors);

            Result result;
            if (job.Type == JobTypeEnums.ResourceBuild)
            {
                var layoutBuildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId, true));
                var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content);
                var normalBuildPlan = GetNormalBuildPlan(villageId, resourceBuildPlan, layoutBuildings);
                if (normalBuildPlan is null)
                {
                    await deleteJobByIdCommand.HandleAsync(new(villageId, job.Id), cancellationToken);
                }
                else
                {
                    await addJobCommand.HandleAsync(new(villageId, normalBuildPlan.ToJob(villageId), true));
                }
                await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
                return Continue.Error;
            }

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

            var dorf = plan.Location < 19 ? 1 : 2;
            result = await toDorfCommand.HandleAsync(new(accountId, dorf), cancellationToken);
            if (result.IsFailed) return result;

            var updateBuildingCommandResult = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            if (updateBuildingCommandResult.IsFailed) return result;

            var (buildings, queueBuildings) = updateBuildingCommandResult.Value;
            if (IsJobComplete(job, buildings, queueBuildings))
            {
                await deleteJobByIdCommand.HandleAsync(new(villageId, job.Id), cancellationToken);
                await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
                return Continue.Error;
            }

            return plan;
        }

        private static NormalBuildPlan GetNormalBuildPlan(
            VillageId villageId,
            ResourceBuildPlan plan,
            List<BuildingItem> layoutBuildings
        )
        {
            layoutBuildings = layoutBuildings
                .Where(x => GetJobQuery.ResourceTypes.Contains(x.Type))
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

            var normalBuildPlan = new NormalBuildPlan()
            {
                Type = chosenOne.Type,
                Level = chosenOne.Level + 1,
                Location = chosenOne.Location,
            };
            return normalBuildPlan;
        }

        private static bool IsJobComplete(JobDto job, List<BuildingDto> buildings, List<QueueBuilding> queueBuildings)
        {
            if (job.Type == JobTypeEnums.ResourceBuild) return false;

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

            var queueBuilding = queueBuildings
                .Where(x => x.Location == plan.Location)
                .OrderByDescending(x => x.Level)
                .Select(x => x.Level)
                .FirstOrDefault();

            if (queueBuilding >= plan.Level) return true;

            var villageBuilding = buildings
                .Where(x => x.Location == plan.Location)
                .Select(x => x.Level)
                .FirstOrDefault();
            if (villageBuilding >= plan.Level) return true;

            return false;
        }
    }
}