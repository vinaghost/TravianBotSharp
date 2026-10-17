using MainCore.Commands.Update;

namespace MainCore.Commands.Navigate
{
    [Handler]
    public sealed partial class ToBuildingByTypeCommand(
        IDbContextFactory<AppDbContext> contextFactory,
        ToDorfCommand.Handler toDorfCommand,
        UpdateBuildingCommand.Handler updateBuildingCommand,
        ToBuildingByLocationCommand.Handler toBuildingByLocationCommand)
    {
        public sealed record Command(VillageId VillageId, BuildingEnums BuildingType) : IVillageCommand;

        private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var (villageId, buildingType) = command;
            Result result;

            result = await toDorfCommand.HandleAsync(new(2), cancellationToken);
            if (result.IsFailed) return result;

            result = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (result.IsFailed) return result;

            using var context = contextFactory.CreateDbContext();
            var location = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == buildingType)
                .Select(x => x.Location)
                .FirstOrDefault();

            if (location == default)
            {
                return MissingBuilding.Error(buildingType);
            }

            result = await toBuildingByLocationCommand.HandleAsync(new(location), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}