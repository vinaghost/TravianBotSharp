namespace MainCore.Commands.Navigate
{
    [Handler]
    public static partial class SwitchManagementTabCommand
    {
        public sealed record Command(VillageId VillageId, int Location) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            var (villageId, location) = command;
            var building = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault(x => x.Location == location);

            if (building is null) return Result.Ok();

            Result result;
            if (building.Type == BuildingEnums.Site)
            {
                var tabIndex = building.Type.GetBuildingsCategory();

                result = await SwitchTabCommand.SwitchTab(browser, tabIndex, cancellationToken);
                if (result.IsFailed) return result;
            }
            else
            {
                if (building.Level < 1) return Result.Ok();
                if (!building.Type.HasMultipleTabs()) return Result.Ok();

                result = await SwitchTabCommand.SwitchTab(browser, 0, cancellationToken);
                if (result.IsFailed) return result;
            }

            return Result.Ok();
        }
    }
}