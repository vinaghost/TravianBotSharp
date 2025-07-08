namespace MainCore.Commands.Features.NpcResource
{
    [Handler]
    public static partial class ToNpcResourcePageCommand
    {
        public sealed record Command(VillageId VillageId) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingCommand.Handler toBuildingCommand,
            SwitchTabCommand.Handler switchTabCommand,
            CancellationToken cancellationToken)
        {
            var villageId = command.VillageId;

            var result = await toDorfCommand.HandleAsync(new(2), cancellationToken);
            if (result.IsFailed) return result;

            var (_, isFailed, response, errors) = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            var marketLocation = response.Buildings
                .Where(x => x.Type == BuildingEnums.Marketplace)
                .Select(x => x.Location)
                .FirstOrDefault();

            if (marketLocation == default)
            {
                return MissingBuilding.Error(BuildingEnums.Marketplace);
            }

            result = await toBuildingCommand.HandleAsync(new(marketLocation), cancellationToken);
            if (result.IsFailed) return result;

            result = await switchTabCommand.HandleAsync(new(0), cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}