namespace MainCore.Commands.Navigate
{
    [Handler]
    public sealed partial class ToBuildingByTypeCommand(
        IDbContextFactory<AppDbContext> contextFactory,
        ToBuildingByLocationCommand.Handler toBuildingByLocationCommand)
    {
        public sealed record Command(VillageId VillageId, BuildingEnums Type) : IVillageCommand;

        private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            using var context = contextFactory.CreateDbContext();
            var location = context.Buildings
                .Where(x => x.VillageId == command.VillageId.Value)
                .Where(x => x.Type == command.Type)
                .Select(x => x.Location)
                .FirstOrDefault();

            if (location == default)
            {
                return MissingBuilding.Error(command.Type);
            }

            return await toBuildingByLocationCommand.HandleAsync(new(location), cancellationToken);
        }
    }
}