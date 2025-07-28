namespace MainCore.Commands.Navigate
{
    [Handler]
    public static partial class ToBuildingByTypeCommand
    {
        public sealed record Command(BuildingEnums Type) : ICommand;

        private static async ValueTask<Result> HandleAsync(
           Command command,
           IChromeBrowser browser,
           AppDbContext context,
           CancellationToken cancellationToken
           )
        {
            var marketLocation = context.Buildings
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