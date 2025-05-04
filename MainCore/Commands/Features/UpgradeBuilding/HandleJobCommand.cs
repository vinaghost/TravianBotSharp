using MainCore.Commands.Base;
using MainCore.Common.Errors.AutoBuilder;
using MainCore.Common.Models;
using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class HandleJobCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : ICommand;

        private static async ValueTask<Result<NormalBuildPlan>> HandleAsync(
            Command command,
            IDbContextFactory<AppDbContext> contextFactory,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery,
            DeleteJobByIdCommand.Handler deleteJobByIdCommand,
            AddJobCommand.Handler addJobCommand,
            CancellationToken cancellationToken
        )
        {
            var (accountId, villageId) = command;
            using var context = await contextFactory.CreateDbContextAsync();

            var (_, isFailed, job, errors) = context.GetJob(accountId, villageId);
            if (isFailed) return Result.Fail(errors);

            Result result;
            if (job.Type == JobTypeEnums.ResourceBuild)
            {
                var layoutBuildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId, true));
                var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content);
                var normalBuildPlan = GetNormalBuildPlan(villageId, resourceBuildPlan, layoutBuildings);
                if (normalBuildPlan is null)
                {
                    await deleteJobByIdCommand.HandleAsync(new(accountId, villageId, job.Id), cancellationToken);
                }
                else
                {
                    await addJobCommand.HandleAsync(new(accountId, villageId, normalBuildPlan.ToJob(villageId), true));
                }
                return Continue.Error;
            }

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

            var dorf = plan.Location < 19 ? 1 : 2;
            result = await toDorfCommand.HandleAsync(new(accountId, dorf), cancellationToken);
            if (result.IsFailed) return result;

            result = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            if (result.IsFailed) return result;

            if (context.IsJobComplete(villageId, job))
            {
                await deleteJobByIdCommand.HandleAsync(new(accountId, villageId, job.Id), cancellationToken);
                return Continue.Error;
            }

            return plan;
        }

        private static List<JobTypeEnums> BuildJobTypes = [
            JobTypeEnums.NormalBuild,
            JobTypeEnums.ResourceBuild
        ];

        private static readonly List<BuildingEnums> ResourceTypes = [
            BuildingEnums.Woodcutter,
            BuildingEnums.ClayPit,
            BuildingEnums.IronMine,
            BuildingEnums.Cropland,
        ];

        private static Result<JobDto> GetJob(this AppDbContext context, AccountId accountId, VillageId villageId)
        {
            var countJob = context.Jobs
              .Where(x => x.VillageId == villageId.Value)
              .Where(x => BuildJobTypes.Contains(x.Type))
              .Count();
            if (countJob == 0) return Skip.AutoBuilderJobQueueEmpty;

            var countQueueBuilding = context.QueueBuildings
               .Where(x => x.VillageId == villageId.Value)
               .Where(x => x.Type != BuildingEnums.Site)
               .Count();

            if (countQueueBuilding == 0)
            {
                var result = context.GetBuildingJob(villageId, false);
                return result;
            }

            var plusActive = context.AccountsInfo
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => x.HasPlusAccount)
                .FirstOrDefault();
            var applyRomanQueueLogic = context.BooleanByName(villageId, VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding);

            if (countQueueBuilding == 1)
            {
                if (plusActive)
                {
                    var result = context.GetBuildingJob(villageId, false);
                    return result;
                }

                if (applyRomanQueueLogic)
                {
                    var result = context.GetBuildingJob(villageId, true);
                    return result;
                }

                return BuildingQueueFull.Error(plusActive, countQueueBuilding);
            }

            if (countQueueBuilding == 2)
            {
                if (plusActive && applyRomanQueueLogic)
                {
                    var result = context.GetBuildingJob(villageId, true);
                    return result;
                }
                return BuildingQueueFull.Error(plusActive, countQueueBuilding);
            }

            return BuildingQueueFull.Error(plusActive, countQueueBuilding);
        }

        private static Result<JobDto> GetBuildingJob(this AppDbContext context, VillageId villageId, bool romanLogic)
        {
            JobDto job;
            if (romanLogic)
            {
                var romanResult = context.GetJobBasedOnRomanLogic(villageId);
                if (romanResult.IsFailed) return Result.Fail(romanResult.Errors);
                job = romanResult.Value;
            }
            else
            {
                job = context.GetBuildingJob(villageId);
            }

            if (job.Type == JobTypeEnums.ResourceBuild) return job;

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);
            if (plan.Type.IsResourceField()) return job;
            var (isSuccess, _, erros) = context.IsJobValid(villageId, plan);
            if (isSuccess) return job;
            return Result.Fail(erros);
        }

        private static JobDto GetBuildingJob(
            this AppDbContext context,
            VillageId villageId)
        {
            var job = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => BuildJobTypes.Contains(x.Type))
                .OrderBy(x => x.Position)
                .ToDto()
                .First();
            return job;
        }

        private static Result<JobDto> GetJobBasedOnRomanLogic(
            this AppDbContext context,
            VillageId villageId)
        {
            var countQueueBuilding = context.CountQueueBuilding(villageId);
            var countResourceQueueBuilding = context.CountResourceQueueBuilding(villageId);
            var countInfrastructureQueueBuilding = countQueueBuilding - countResourceQueueBuilding;
            if (countResourceQueueBuilding > countInfrastructureQueueBuilding)
            {
                var job = context.GetInfrastructureBuildingJob(villageId);
                if (job is null) return JobNotAvailable.Error("Infrastructure building");
                return job;
            }
            else
            {
                var job = context.GetResourceBuildingJob(villageId);
                if (job is null) return JobNotAvailable.Error("Resource field");
                return job;
            }
        }

        private static int CountQueueBuilding(
            this AppDbContext context,
            VillageId villageId)
        {
            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .Count();
            return count;
        }

        private static int CountResourceQueueBuilding(
            this AppDbContext context,
            VillageId villageId)
        {
            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => ResourceTypes.Contains(x.Type))
                .Count();
            return count;
        }

        private static JobDto GetInfrastructureBuildingJob(
            this AppDbContext context,
            VillageId villageId)
        {
            var job = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == JobTypeEnums.NormalBuild)
                .ToDto()
                .AsEnumerable()
                .Select(x => new
                {
                    Job = x,
                    Content = JsonSerializer.Deserialize<NormalBuildPlan>(x.Content)
                })
                .Where(x => !ResourceTypes.Contains(x.Content.Type))
                .Select(x => x.Job)
                .OrderBy(x => x.Position)
                .FirstOrDefault();
            return job;
        }

        private static JobDto GetResourceBuildingJob(
            this AppDbContext context,
            VillageId villageId)
        {
            var job = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == JobTypeEnums.NormalBuild)
                .ToDto()
                .AsEnumerable()
                .Select(x => new
                {
                    Job = x,
                    Content = JsonSerializer.Deserialize<NormalBuildPlan>(x.Content)
                })
                .Where(x => ResourceTypes.Contains(x.Content.Type))
                .Select(x => x.Job)
                .OrderBy(x => x.Position)
                .FirstOrDefault();

            var resourceBuildJob = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == JobTypeEnums.ResourceBuild)
                .ToDto()
                .FirstOrDefault();

            if (job is null) return resourceBuildJob;
            if (resourceBuildJob is null) return job;
            if (job.Position < resourceBuildJob.Position) return job;
            return resourceBuildJob;
        }

        private static Result IsJobValid(
            this AppDbContext context,
            VillageId villageId,
            NormalBuildPlan plan)
        {
            var currentBuilding = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == plan.Location)
                .FirstOrDefault();

            if (currentBuilding is not null && currentBuilding.Type == plan.Type) return Result.Ok();

            var prerequisiteBuildings = plan.Type.GetPrerequisiteBuildings();

            var errors = new List<PrerequisiteBuildingMissing>();

            var buildings = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => prerequisiteBuildings.Select(x => x.Type).Contains(x.Type))
                .ToList();

            foreach (var prerequisiteBuilding in prerequisiteBuildings)
            {
                var vaild = buildings
                    .Where(x => x.Type == prerequisiteBuilding.Type)
                    .Any(x => x.Level >= prerequisiteBuilding.Level);
                if (!vaild) errors.Add(new(prerequisiteBuilding.Type, prerequisiteBuilding.Level));
            }

            if (!plan.Type.IsMultipleBuilding()) return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();

            var firstBuilding = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == plan.Type)
                .OrderByDescending(x => x.Level)
                .FirstOrDefault();

            if (firstBuilding is null) return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();
            if (firstBuilding.Level == firstBuilding.Type.GetMaxLevel()) return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();

            errors.Add(new(firstBuilding.Type, firstBuilding.Level));
            return Result.Fail(errors);
        }

        private static NormalBuildPlan GetNormalBuildPlan(
            VillageId villageId,
            ResourceBuildPlan plan,
            List<BuildingItem> layoutBuildings
        )
        {
            layoutBuildings = layoutBuildings
                .Where(x => ResourceTypes.Contains(x.Type))
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

        private static bool IsJobComplete(
            this AppDbContext context,
            VillageId villageId,
            JobDto job)
        {
            if (job.Type == JobTypeEnums.ResourceBuild) return false;

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

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