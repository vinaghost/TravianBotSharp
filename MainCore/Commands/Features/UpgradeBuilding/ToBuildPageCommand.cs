using MainCore.Constraints;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class ToBuildPageCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, NormalBuildPlan Plan) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            ToBuildingCommand.Handler toBuildingCommand,
            SwitchTabCommand.Handler switchTabCommand,
            GetBuildingQuery.Handler getBuilding,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId, plan) = command;

            Result result;
            result = await toBuildingCommand.HandleAsync(new(accountId, plan.Location), cancellationToken);
            if (result.IsFailed) return result;

            var building = await getBuilding.HandleAsync(new(villageId, plan.Location), cancellationToken);
            if (building.Type == BuildingEnums.Site)
            {
                var tabIndex = plan.Type.GetBuildingsCategory();
                result = await switchTabCommand.HandleAsync(new(accountId, tabIndex), cancellationToken);
                if (result.IsFailed) return result;
            }
            else
            {
                if (building.Level < 1) return Result.Ok();
                if (!building.Type.HasMultipleTabs()) return Result.Ok();
                result = await switchTabCommand.HandleAsync(new(accountId, 0), cancellationToken);
                if (result.IsFailed) return result;
            }
            return Result.Ok();
        }
    }
}