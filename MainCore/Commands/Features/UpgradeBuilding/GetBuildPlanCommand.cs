using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class GetBuildPlanCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private static async ValueTask<Result<NormalBuildPlan>> HandleAsync(
            Command command,
            GetJobCommand.Handler getJobQuery,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            GetLayoutBuildingsCommand.Handler getLayoutBuildingsQuery,
            DeleteJobByIdCommand.Handler deleteJobByIdCommand,
            AddJobCommand.Handler addJobCommand,
            ValidatePlanCompleteCommand.Handler validatePlanCompleteCommand,
            ILogger logger,
            IRxQueue rxQueue,
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
                    logger.Information("{Content}", job);

                    var layoutBuildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId, true));
                    var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content)!;
                    var normalBuildPlan = GetNormalBuildPlan(resourceBuildPlan, layoutBuildings);
                    if (normalBuildPlan is null)
                    {
                        await deleteJobByIdCommand.HandleAsync(new(job.Id), cancellationToken);
                    }
                    else
                    {
                        await addJobCommand.HandleAsync(new(villageId, normalBuildPlan.ToJob(), true));
                    }
                    rxQueue.Enqueue(new JobsModified(villageId));
                    continue;
                }

                var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;
                Result result;
                if (plan.Type.IsResourceBonus())
                {
                    result = await toDorfCommand.HandleAsync(new(1), cancellationToken);
                    if (result.IsFailed) return result;

                    result = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
                    if (result.IsFailed) return result;

                    result = await toDorfCommand.HandleAsync(new(2), cancellationToken);
                    if (result.IsFailed) return result;

                    result = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
                    if (result.IsFailed) return result;
                }
                else
                {
                    var dorf = plan.Location < 19 ? 1 : 2;
                    result = await toDorfCommand.HandleAsync(new(dorf), cancellationToken);
                    if (result.IsFailed) return result;

                    result = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
                    if (result.IsFailed) return result;
                }

                var validateResult = await validatePlanCompleteCommand.HandleAsync(new(villageId, plan), cancellationToken);
                if (validateResult.IsFailed) return Result.Fail(validateResult.Errors);
                if (!validateResult.Value)
                {
                    await deleteJobByIdCommand.HandleAsync(new(job.Id), cancellationToken);
                    rxQueue.Enqueue(new JobsModified(villageId));
                    continue;
                }

                return plan;
            }
        }

        private static NormalBuildPlan? GetNormalBuildPlan(
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

            if (resourceFields.Count == 0) return null;

            var minLevel = resourceFields
                .Select(x => x.Level)
                .Min();

            var chosenOne = resourceFields
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
    }
}