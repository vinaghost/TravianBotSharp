namespace MainCore.Commands.Navigate
{
    [Handler]
    public static partial class ToBuildingByTypeCommand
    {
        public sealed record Command(VillageId VillageId, BuildingEnums Type) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
           Command command,
           IChromeBrowser browser,
           AppDbContext context,
           CancellationToken cancellationToken
           )
        {
            var marketLocation = context.Buildings
                .Where(x => x.VillageId == command.VillageId.Value)
                .Where(x => x.Type == command.Type)
                .Select(x => x.Location)
                .FirstOrDefault();

            if (marketLocation == default)
            {
                return MissingBuilding.Error(command.Type);
            }

            return await ToBuildingByLocationCommand.ToBuilding(marketLocation, browser, cancellationToken);
        }
    }
}