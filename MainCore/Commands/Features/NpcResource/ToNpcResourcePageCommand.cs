using MainCore.Common.Errors.TrainTroop;

namespace MainCore.Commands.Features.NpcResource
{
    [Handler]
    public static partial class ToNpcResourcePageCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : ICustomCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeManager chromeManager,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingCommand.Handler toBuildingCommand,
            SwitchTabCommand.Handler switchTabCommand,
            GetBuildingLocationQuery.Handler getBuildingLocation,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId) = command;

            var result = await toDorfCommand.HandleAsync(new(accountId, 2), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var market = await getBuildingLocation.HandleAsync(new(villageId, BuildingEnums.Marketplace), cancellationToken);
            if (market == default)
            {
                return MissingBuilding.Error(BuildingEnums.Marketplace);
            }

            result = await toBuildingCommand.HandleAsync(new(accountId, market), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await switchTabCommand.HandleAsync(new(accountId, 0), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}