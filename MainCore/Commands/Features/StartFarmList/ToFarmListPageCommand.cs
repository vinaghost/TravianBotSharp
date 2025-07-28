namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class ToFarmListPageCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            GetHasRallypointVillageCommand.Handler getHasRallypointVillageCommand,
            SwitchVillageCommand.Handler switchVillageCommand,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingByLocationCommand.Handler toBuildingCommand,
            SwitchTabCommand.Handler switchTabCommand,
            IDelayService delayService,
            CancellationToken cancellationToken)
        {
            var accountId = command.AccountId;
            var rallypointVillageId = await getHasRallypointVillageCommand.HandleAsync(new(accountId), cancellationToken);
            if (rallypointVillageId == VillageId.Empty) return Skip.NoRallypoint;

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
    }
}