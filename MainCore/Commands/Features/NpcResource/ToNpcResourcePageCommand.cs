namespace MainCore.Commands.Features.NpcResource
{
    [Handler]
    public static partial class ToNpcResourcePageCommand
    {
        public sealed record Command(VillageId VillageId) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            ToBuildingByTypeCommand.Handler toBuildingCommand,
            SwitchTabCommand.Handler switchTabCommand,
            CancellationToken cancellationToken)
        {
            var villageId = command.VillageId;

            var result = await toDorfCommand.HandleAsync(new(2), cancellationToken);
            if (result.IsFailed) return result;

            var (_, isFailed, errors) = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            result = await toBuildingCommand.HandleAsync(new(BuildingEnums.Marketplace), cancellationToken);
            if (result.IsFailed) return result;

            result = await switchTabCommand.HandleAsync(new(0), cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}