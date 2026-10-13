using MainCore.Commands.Navigate;
using MainCore.Commands.Update;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public sealed partial class ToBuildPageCommand(
        UpdateBuildingCommand.Handler updateBuildingCommand,
        ToBuildingByLocationCommand.Handler toBuildingCommand,
        SwitchTabCommand.Handler switchTabCommand,
        IDbContextFactory<AppDbContext> contextFactory)
    {
        public sealed record Command(VillageId VillageId, NormalBuildPlan Plan) : IVillageCommand;

        private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var (villageId, plan) = command;

            var (_, isFailed, errors) = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            Result result;
            result = await toBuildingCommand.HandleAsync(new(plan.Location), cancellationToken);
            if (result.IsFailed) return result;

            result = await SwitchManagementTab(villageId, plan, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private async ValueTask<Result> SwitchManagementTab(VillageId villageId, NormalBuildPlan plan, CancellationToken cancellationToken)
        {
            using var context = contextFactory.CreateDbContext();
            var building = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault(x => x.Location == plan.Location);

            if (building is null) return Result.Ok();

            Result result;
            if (building.Type == BuildingEnums.Site)
            {
                var tabIndex = plan.Type.GetBuildingsCategory();

                result = await switchTabCommand.HandleAsync(new(tabIndex), cancellationToken);
                if (result.IsFailed) return result;
            }
            else
            {
                if (building.Level < 1) return Result.Ok();
                if (!building.Type.HasMultipleTabs()) return Result.Ok();

                result = await switchTabCommand.HandleAsync(new(0), cancellationToken);
                if (result.IsFailed) return result;
            }

            return Result.Ok();
        }
    }
}