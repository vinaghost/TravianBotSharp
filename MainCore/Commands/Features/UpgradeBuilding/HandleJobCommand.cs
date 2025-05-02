using MainCore.Common.Errors.AutoBuilder;
using MainCore.Common.Models;
using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class HandleJobCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, JobDto Job) : ICustomCommand;

        private static async ValueTask<Result<NormalBuildPlan>> HandleAsync(
            Command command,
            IGetSetting getSetting,
            IDbContextFactory<AppDbContext> contextFactory,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery,
            DeleteJobByIdCommand.Handler deleteJobByIdCommand,
            AddJobCommand.Handler addJobCommand,
            CancellationToken cancellationToken
        )
        {
            var (accountId, villageId, job) = command;

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
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            using var context = await contextFactory.CreateDbContextAsync();
            if (IsJobComplete(context, villageId, job))
            {
                await deleteJobByIdCommand.HandleAsync(new(accountId, villageId, job.Id), cancellationToken);
                return Continue.Error;
            }

            return plan;
        }

        private static readonly List<BuildingEnums> ResourceTypes = new()
        {
            BuildingEnums.Woodcutter,
            BuildingEnums.ClayPit,
            BuildingEnums.IronMine,
            BuildingEnums.Cropland,
        };

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

        private static bool IsJobComplete(AppDbContext context, VillageId villageId, JobDto job)
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