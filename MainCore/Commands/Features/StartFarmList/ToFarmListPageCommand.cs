using MainCore.Constraints;

namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class ToFarmListPageCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            GetVillageHasRallypointQuery.Handler getVillageHasRallypointQuery,
            SwitchVillageCommand.Handler switchVillageCommand,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingCommand.Handler toBuildingCommand,
            SwitchTabCommand.Handler switchTabCommand,
            DelayClickCommand.Handler delayClickCommand,
            CancellationToken cancellationToken)
        {
            var rallypointVillageId = await getVillageHasRallypointQuery.HandleAsync(new(command.AccountId), cancellationToken);
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