namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class ToBuildPageCommand
    {
        public sealed record Command(VillageId VillageId, NormalBuildPlan Plan) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            ToBuildingCommand.Handler toBuildingCommand,
            SwitchTabCommand.Handler switchTabCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            IDelayService delayService,
            CancellationToken cancellationToken)
        {
            var (villageId, plan) = command;

            var (_, isFailed, (buildings, _), errors) = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            Result result;
            result = await toBuildingCommand.HandleAsync(new(plan.Location), cancellationToken);
            if (result.IsFailed) return result;

            await delayService.DelayClick(cancellationToken);

            var building = buildings.First(x => x.Location == plan.Location);

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

            await delayService.DelayClick(cancellationToken);

            return Result.Ok();
        }
    }
}