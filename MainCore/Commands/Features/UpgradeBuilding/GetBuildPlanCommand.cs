﻿using MainCore.Commands.UI.Villages.BuildViewModel;
using MainCore.Constraints;
using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class GetBuildPlanCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private static async ValueTask<Result<NormalBuildPlan>> HandleAsync(
            Command command,
            GetJobQuery.Handler getJobQuery,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery,
            DeleteJobByIdCommand.Handler deleteJobByIdCommand,
            AddJobCommand.Handler addJobCommand,
            JobUpdated.Handler jobUpdated,
            ILogger logger,
            CancellationToken cancellationToken
        )
        {
            var (accountId, villageId) = command;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;

                var (_, isFailed, job, errors) = await getJobQuery.HandleAsync(new(accountId, villageId), cancellationToken);
                if (isFailed) return Result.Fail(errors);

                if (job.Type == JobTypeEnums.ResourceBuild)
                {
                    logger.Information("{Content}", job.GetContent());

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
                    continue;
                }

                var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;

                var dorf = plan.Location < 19 ? 1 : 2;
                var result = await toDorfCommand.HandleAsync(new(accountId, dorf), cancellationToken);
                if (result.IsFailed) return result;

                (_, isFailed, var updateBuildingValue, errors) = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
                if (isFailed) return Result.Fail(errors);

                var (buildings, queueBuildings) = updateBuildingValue;
                if (IsJobComplete(job, buildings, queueBuildings))
                {
                    await deleteJobByIdCommand.HandleAsync(new(villageId, job.Id), cancellationToken);
                    await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
                    continue;
                }

                return plan;
            }
        }

        private static NormalBuildPlan? GetNormalBuildPlan(
            VillageId villageId,
            ResourceBuildPlan plan,
            List<BuildingItem> layoutBuildings
        )
        {
            List<BuildingItem> resourceFields;

            if (plan.Plan == ResourcePlanEnums.ExcludeCrop)
            {
                resourceFields = layoutBuildings
                    .Where(x => x.Type == BuildingEnums.Woodcutter || x.Type == BuildingEnums.ClayPit || x.Type == BuildingEnums.IronMine)
                    .Where(x => x.Level < plan.Level)
                    .ToList();
            }
            else if (plan.Plan == ResourcePlanEnums.OnlyCrop)
            {
                resourceFields = layoutBuildings
                    .Where(x => x.Type == BuildingEnums.Cropland)
                    .Where(x => x.Level < plan.Level)
                    .ToList();
            }
            else
            {
                resourceFields = layoutBuildings
                    .Where(x => x.Type.IsResourceField())
                    .Where(x => x.Level < plan.Level)
                    .ToList();
            }

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

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;

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