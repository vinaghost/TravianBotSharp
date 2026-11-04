using MainCore.Commands.Update;

namespace MainCore.Commands.Navigate
{
    [Handler]
    public sealed partial class ToBuildingByTypeCommand(
        ToDorfCommand.Handler toDorfCommand,
        ToBuildingByLocationCommand.Handler toBuildingByLocationCommand)
    {
        public sealed record Command(IChromeBrowser Browser, BuildingEnums BuildingType) : ICommand
        {
            public string Name => $"{nameof(ToBuildingByTypeCommand)} [{Description}]";
            private string Description => $"{BuildingType}";
        }

        private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var (browser, buildingType) = command;
            Result result;

            result = await toDorfCommand.HandleAsync(new(browser, 2), cancellationToken);
            if (result.IsFailed) return result;

            var buildingLocator = GetBuildingLocator(browser.CurrentPage, buildingType);

            if (await buildingLocator.CountAsync() == 0)
            {
                return MissingBuilding.Error(buildingType);
            }

            var location = await GetBuildingLocation(buildingLocator);

            result = await toBuildingByLocationCommand.HandleAsync(new(browser, location), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private static ILocator GetBuildingLocator(IPage page, BuildingEnums buildingType)
        {
            return page.Locator($".buildingSlot.g{buildingType}");
        }

        private static async Task<int> GetBuildingLocation(ILocator locator)
        {
            var location = await locator.GetAttributeAsync("data-aid");
            return int.Parse(location ?? "0");
        }
    }
}