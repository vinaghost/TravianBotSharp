using MainCore.Infrasturecture.Extensions;
using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public sealed partial class GetBuildPlanCommand(
        IDbContextFactory<AppDbContext> contextFactory,
        ToDorfCommand.Handler toDorfCommand,
        UpdateBuildingCommand.Handler updateBuildingCommand,
        ILogger logger,
        RxQueue rxQueue)
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private async ValueTask<Result<NormalBuildPlan>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var (accountId, villageId) = command;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;

                var (_, isFailed, job, errors) = GetJob(accountId, villageId);
                if (isFailed) return Result.Fail(errors);

                if (job.Type == JobTypeEnums.ResourceBuild)
                {
                    logger.Information("{Content}", job);

                    var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content)!;
                    var normalBuildPlan = GetNormalBuildPlan(villageId, resourceBuildPlan);
                    using var context = contextFactory.CreateDbContext();
                    if (normalBuildPlan is null)
                    {
                        context.DeleteJobById(job.Id);
                    }
                    else
                    {
                        context.AddJob(villageId, normalBuildPlan, true);
                    }
                    rxQueue.Enqueue(new JobsModified(accountId, villageId));
                    continue;
                }

                var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;
                Result result;
                if (plan.Type.IsResourceBonus())
                {
                    result = await CheckBonusBuilding(villageId, cancellationToken);
                    if (result.IsFailed) return result;
                }
                else
                {
                    result = await CheckBuilding(villageId, plan.Location, cancellationToken);
                    if (result.IsFailed) return result;
                }

                var isComplete = IsBuildingComplete(villageId, plan);
                if (isComplete)
                {
                    using (var context = contextFactory.CreateDbContext())
                    {
                        context.DeleteJobById(job.Id);
                    }
                    rxQueue.Enqueue(new JobsModified(accountId, villageId));
                    continue;
                }

                result = CheckPrerequisite(villageId, plan);
                if (result.IsFailed) return result;

                return plan;
            }
        }

        private Result<JobDto> GetJob(AccountId accountId, VillageId villageId)
        {
            using var context = contextFactory.CreateDbContext();
            return context.GetJob(accountId, villageId);
        }

        private async Task<Result> CheckBonusBuilding(VillageId villageId, CancellationToken cancellationToken)
        {
            var result = await toDorfCommand.HandleAsync(new(1), cancellationToken);
            if (result.IsFailed) return result;

            result = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (result.IsFailed) return result;

            result = await toDorfCommand.HandleAsync(new(2), cancellationToken);
            if (result.IsFailed) return result;

            result = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private async Task<Result> CheckBuilding(VillageId villageId, int location, CancellationToken cancellationToken)
        {
            var dorf = location < 19 ? 1 : 2;
            var result = await toDorfCommand.HandleAsync(new(dorf), cancellationToken);
            if (result.IsFailed) return result;
            result = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private bool IsBuildingComplete(VillageId villageId, NormalBuildPlan plan)
        {
            using var context = contextFactory.CreateDbContext();
            var completeQueueBuildings = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.CompleteTime < DateTime.Now)
                .OrderBy(x => x.Level)
                .ToList();

            if (completeQueueBuildings.Count > 0)
            {
                foreach (var completeQueueBuilding in completeQueueBuildings)
                {
                    if (completeQueueBuilding.Location == -1) continue;

                    var building = context.Buildings
                        .Where(x => x.VillageId == villageId.Value)
                        .FirstOrDefault(x => x.Location == completeQueueBuilding.Location);
                    if (building is null) continue;

                    building.Level = completeQueueBuilding.Level;
                    context.Remove(completeQueueBuilding);
                }
                context.SaveChanges();
            }

            var oldBuilding = context.Buildings
                .AsNoTracking()
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault(x => x.Location == plan.Location);

            if (oldBuilding is not null && oldBuilding.Type == plan.Type)
            {
                if (oldBuilding.Level >= plan.Level) return true;
                var queueBuilding = context.QueueBuildings
                    .AsNoTracking()
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Location == plan.Location)
                    .OrderByDescending(x => x.Level)
                    .Select(x => x.Level)
                    .FirstOrDefault();

                if (queueBuilding >= plan.Level) return true;
                return false;
            }

            return false;
        }

        private Result CheckPrerequisite(VillageId villageId, NormalBuildPlan plan)
        {
            using var context = contextFactory.CreateDbContext();
            var buildings = context.Buildings
               .AsNoTracking()
               .Where(x => x.VillageId == villageId.Value)
               .ToList();

            var queueBuildings = context.QueueBuildings
                .AsNoTracking()
                .Where(x => x.VillageId == villageId.Value)
                .OrderBy(x => x.CompleteTime)
                .ToList();

            var errors = new List<IError>();
            var prerequisiteBuildings = plan.Type.GetPrerequisiteBuildings();

            foreach (var prerequisiteBuilding in prerequisiteBuildings)
            {
                var vaild = buildings
                   .Any(x => x.Type == prerequisiteBuilding.Type && x.Level >= prerequisiteBuilding.Level);

                if (!vaild)
                {
                    errors.Add(UpgradeBuildingError.PrerequisiteBuildingMissing(prerequisiteBuilding.Type, prerequisiteBuilding.Level));
                    var queueBuilding = queueBuildings.Find(x => x.Type == prerequisiteBuilding.Type && x.Level == prerequisiteBuilding.Level);
                    if (queueBuilding is not null)
                    {
                        errors.Add(NextExecuteError.PrerequisiteBuildingInQueue(prerequisiteBuilding.Type, prerequisiteBuilding.Level, queueBuilding.CompleteTime));
                    }
                }
            }

            if (!plan.Type.IsMultipleBuilding()) return Result.FailIfNotEmpty(errors);

            var firstBuilding = buildings
                .Where(x => x.Type == plan.Type)
                .OrderByDescending(x => x.Level)
                .FirstOrDefault();

            if (firstBuilding is null) return Result.FailIfNotEmpty(errors);
            if (firstBuilding.Location == plan.Location) return Result.FailIfNotEmpty(errors);
            if (firstBuilding.Level == firstBuilding.Type.GetMaxLevel()) return Result.FailIfNotEmpty(errors);

            errors.Add(UpgradeBuildingError.PrerequisiteBuildingMissing(firstBuilding.Type, firstBuilding.Level));
            var prerequisiteBuildingUndercontruction = queueBuildings.Find(x => x.Type == firstBuilding.Type && x.Level == firstBuilding.Level);
            if (prerequisiteBuildingUndercontruction is not null)
            {
                errors.Add(NextExecuteError.PrerequisiteBuildingInQueue(firstBuilding.Type, firstBuilding.Level, prerequisiteBuildingUndercontruction.CompleteTime));
            }

            return Result.FailIfNotEmpty(errors);
        }

        private NormalBuildPlan? GetNormalBuildPlan(VillageId villageId, ResourceBuildPlan plan)
        {
            using var context = contextFactory.CreateDbContext();
            var layoutBuildings = context.GetLayoutBuildings(villageId, true);
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
                .Min(x => x.Level);

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