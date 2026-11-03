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
        public sealed record Command(AccountId AccountId, VillageId VillageId, BuildingEnums BuildingType) : IAccountVillageCommand
        {
            public string Name => $"{nameof(ToBuildingByTypeCommand)} [{Description}]";
            private string Description => $"{BuildingType}";
        }

        private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var (accountId, villageId, buildingType) = command;
            Result result;

            result = await toDorfCommand.HandleAsync(new(accountId, 2), cancellationToken);
            if (result.IsFailed) return result;

            result = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
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

            result = await toBuildingByLocationCommand.HandleAsync(new(accountId, location), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}