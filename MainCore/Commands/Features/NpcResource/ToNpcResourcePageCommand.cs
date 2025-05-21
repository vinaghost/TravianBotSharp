using MainCore.Constraints;
using MainCore.Errors.TrainTroop;

namespace MainCore.Commands.Features.NpcResource
{
    [Handler]
    public static partial class ToNpcResourcePageCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingCommand.Handler toBuildingCommand,
            SwitchTabCommand.Handler switchTabCommand,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId) = command;

            var result = await toDorfCommand.HandleAsync(new(accountId, 2), cancellationToken);
            if (result.IsFailed) return result;

            var (_, isFailed, response, errors) = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            var marketLocation = response.Buildings
                .Where(x => x.Type == BuildingEnums.Marketplace)
                .Select(x => x.Location)
                .FirstOrDefault();

            if (marketLocation == default)
            {
                return MissingBuilding.Error(BuildingEnums.Marketplace);
            }

            result = await toBuildingCommand.HandleAsync(new(accountId, marketLocation), cancellationToken);
            if (result.IsFailed) return result;

            result = await switchTabCommand.HandleAsync(new(accountId, 0), cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}