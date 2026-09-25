namespace MainCore.Commands.Navigate
{
    [Handler]
    public sealed partial class ToFarmListPageCommand(
        IDbContextFactory<AppDbContext> contextFactory,
        SwitchVillageCommand.Handler switchVillageCommand,
        ToDorfCommand.Handler toDorfCommand,
        UpdateBuildingCommand.Handler updateBuildingCommand,
        ToBuildingByLocationCommand.Handler toBuildingCommand,
        SwitchTabCommand.Handler switchTabCommand,
        IDelayService delayService)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private async ValueTask<Result> HandleAsync(
            Command command,
            CancellationToken cancellationToken)
        {
            var accountId = command.AccountId;
            var rallypointVillageId = GetHasRallypointVillage(accountId);
            if (rallypointVillageId == VillageId.Empty) return Skip.Error.WithError("No rallypoint found. Recheck & load village has rallypoint in Village>Build tab");

            var result = await switchVillageCommand.HandleAsync(new(rallypointVillageId), cancellationToken);
            if (result.IsFailed) return result;

            result = await toDorfCommand.HandleAsync(new(2), cancellationToken);
            if (result.IsFailed) return result;

            var (_, isFailed, errors) = await updateBuildingCommand.HandleAsync(new(rallypointVillageId), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            result = await toBuildingCommand.HandleAsync(new(39), cancellationToken);
            if (result.IsFailed) return result;

            await delayService.DelayClick(cancellationToken);

            result = await switchTabCommand.HandleAsync(new(4), cancellationToken);
            if (result.IsFailed) return result;

            await delayService.DelayClick(cancellationToken);
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