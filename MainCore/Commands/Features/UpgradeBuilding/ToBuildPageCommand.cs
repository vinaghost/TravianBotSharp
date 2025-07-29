namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class ToBuildPageCommand
    {
        public sealed record Command(VillageId VillageId, NormalBuildPlan Plan) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingByLocationCommand.Handler toBuildingCommand,
            SwitchManagementTabCommand.Handler switchManagementTabCommand,
            IDelayService delayService,
            CancellationToken cancellationToken)
        {
            var (villageId, plan) = command;

            var (_, isFailed, errors) = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            Result result;
            result = await toBuildingCommand.HandleAsync(new(plan.Location), cancellationToken);
            if (result.IsFailed) return result;

            await delayService.DelayClick(cancellationToken);

            result = await switchManagementTabCommand.HandleAsync(new(villageId, plan.Location), cancellationToken);
            if (result.IsFailed) return result;

            await delayService.DelayClick(cancellationToken);

            return Result.Ok();
        }
    }
}