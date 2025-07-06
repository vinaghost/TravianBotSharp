namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class ToFarmListPageCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            AppDbContext context,
            SwitchVillageCommand.Handler switchVillageCommand,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingCommand.Handler toBuildingCommand,
            SwitchTabCommand.Handler switchTabCommand,
            DelayClickCommand.Handler delayClickCommand,
            CancellationToken cancellationToken)
        {
            var accountId = command.AccountId;
            var rallypointVillageId = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.Buildings.Any(x => x.Type == BuildingEnums.RallyPoint && x.Level > 0))
                .OrderByDescending(x => x.IsActive)
                .Select(x => new VillageId(x.Id))
                .FirstOrDefault();

            if (rallypointVillageId == VillageId.Empty) return Skip.NoRallypoint;

            var result = await switchVillageCommand.HandleAsync(new(command.AccountId, rallypointVillageId), cancellationToken);
            if (result.IsFailed) return result;

            result = await toDorfCommand.HandleAsync(new(command.AccountId, 2), cancellationToken);
            if (result.IsFailed) return result;

            var updateBuildingCommandResult = await updateBuildingCommand.HandleAsync(new(command.AccountId, rallypointVillageId), cancellationToken);
            if (updateBuildingCommandResult.IsFailed) return Result.Fail(updateBuildingCommandResult.Errors);

            result = await toBuildingCommand.HandleAsync(new(command.AccountId, 39), cancellationToken);
            if (result.IsFailed) return result;

            result = await switchTabCommand.HandleAsync(new(command.AccountId, 4), cancellationToken);
            if (result.IsFailed) return result;

            await delayClickCommand.HandleAsync(new(command.AccountId), cancellationToken);
            return Result.Ok();
        }
    }
}