using MainCore.Errors.TrainTroop;

namespace MainCore.Commands.Features.TrainTroop
{
    [Handler]
    public static partial class ToTrainTroopPageCommand
    {
        public sealed record Command(VillageId VillageId, BuildingEnums Building) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingCommand.Handler toBuildingCommand,
            CancellationToken cancellationToken)
        {
            var (villageId, building) = command;

            var result = await toDorfCommand.HandleAsync(new(2), cancellationToken);
            if (result.IsFailed) return result;

            var (_, isFailed, response, errors) = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            var buildingLocation = response.Buildings
                .Where(x => x.Type == building)
                .Select(x => x.Location)
                .FirstOrDefault();

            if (buildingLocation == default) return MissingBuilding.Error(building);

            result = await toBuildingCommand.HandleAsync(new(buildingLocation), cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}