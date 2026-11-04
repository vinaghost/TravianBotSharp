using MainCore.Commands.Update;

namespace MainCore.Commands.Navigate
{
    [Handler]
    public sealed partial class ToFarmListPageCommand(
        IDbContextFactory<AppDbContext> contextFactory,
        ChromeManager chromeManager,
        SwitchVillageCommand.Handler switchVillageCommand,
        ToDorfCommand.Handler toDorfCommand,
        ToBuildingByTypeCommand.Handler toBuildingCommand,
        SwitchTabCommand.Handler switchTabCommand)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => $"{nameof(ToFarmListPageCommand)}";
        }

        private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var accountId = command.AccountId;
            var browser = chromeManager.Get(accountId);

            var rallypointVillageId = GetHasRallypointVillage(accountId);
            if (rallypointVillageId == VillageId.Empty) return Skip.Error.WithError("No rallypoint found. Recheck & load village has rallypoint in Village>Build tab");

            var result = await switchVillageCommand.HandleAsync(new(browser, rallypointVillageId), cancellationToken);
            if (result.IsFailed) return result;

            result = await toDorfCommand.HandleAsync(new(browser, 2), cancellationToken);
            if (result.IsFailed) return result;

            result = await toBuildingCommand.HandleAsync(new(browser, BuildingEnums.RallyPoint), cancellationToken);
            if (result.IsFailed) return result;

            result = await switchTabCommand.HandleAsync(new(browser, 4), cancellationToken);
            if (result.IsFailed) return result;

            result = await browser.Wait(FarmListParser.GetStartAllButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private VillageId GetHasRallypointVillage(AccountId accountId)
        {
            using var context = contextFactory.CreateDbContext();
            var hasRallypointVillageId = context.Villages
               .Where(x => x.AccountId == accountId.Value)
               .Where(x => x.Buildings.Any(x => x.Type == BuildingEnums.RallyPoint && x.Level > 0))
               .OrderByDescending(x => x.IsActive)
               .Select(x => new VillageId(x.Id))
               .FirstOrDefault();
            return hasRallypointVillageId;
        }
    }
}