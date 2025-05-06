using MainCore.Constraints;

namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class ToFarmListPageCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            SwitchVillageCommand.Handler switchVillageCommand,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingCommand.Handler toBuildingCommand,
            SwitchTabCommand.Handler switchTabCommand,
            DelayClickCommand.Handler delayClickCommand,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            var rallypointVillageId = GetVillageHasRallypoint(command.AccountId, context);
            if (rallypointVillageId == VillageId.Empty) return Skip.NoRallypoint;

            var result = await switchVillageCommand.HandleAsync(new(command.AccountId, rallypointVillageId), cancellationToken);
            if (result.IsFailed) return result;

            result = await toDorfCommand.HandleAsync(new(command.AccountId, 2), cancellationToken);
            if (result.IsFailed) return result;

            result = await updateBuildingCommand.HandleAsync(new(command.AccountId, rallypointVillageId), cancellationToken);
            if (result.IsFailed) return result;

            result = await toBuildingCommand.HandleAsync(new(command.AccountId, 39), cancellationToken);
            if (result.IsFailed) return result;

            result = await switchTabCommand.HandleAsync(new(command.AccountId, 4), cancellationToken);
            if (result.IsFailed) return result;

            await delayClickCommand.HandleAsync(new(command.AccountId), cancellationToken);
            return Result.Ok();
        }

        private static VillageId GetVillageHasRallypoint(AccountId accountId, AppDbContext context)
        {

            return context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.Buildings.Any(x => x.Type == BuildingEnums.RallyPoint && x.Level > 0))
                .OrderByDescending(x => x.IsActive)
                .Select(x => new VillageId(x.Id))
                .FirstOrDefault();
        }
    }
}