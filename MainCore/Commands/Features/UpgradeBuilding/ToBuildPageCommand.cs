using MainCore.Constraints;
using MainCore.Specifications;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class ToBuildPageCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, NormalBuildPlan Plan) : IAccountVillageCommand
        {
            public void Deconstruct(out AccountId accountId, out VillageId villageId) => (accountId, villageId) = (AccountId, VillageId);
        }

        private static async ValueTask<Result> HandleAsync(
            Command command,
            ToBuildingCommand.Handler toBuildingCommand,
            SwitchTabCommand.Handler switchTabCommand,
            DelayClickCommand.Handler delayClickCommand,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId, plan) = command;

            Result result;
            result = await toBuildingCommand.HandleAsync(new(accountId, plan.Location), cancellationToken);
            if (result.IsFailed) return result;

            await delayClickCommand.HandleAsync(new(accountId), cancellationToken);

            var spec = new GetBuildingSpec(villageId, plan.Location);
            var building = context.Buildings
                .WithSpecification(spec)
                .ToDto()
                .First();

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

            await delayClickCommand.HandleAsync(new(accountId), cancellationToken);

            return Result.Ok();
        }
    }
}