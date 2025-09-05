namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class GetHasRallypointVillageCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<VillageId> HandleAsync(
            Command command,
            AppDbContext context)
        {
            await Task.CompletedTask;
            var accountId = command.AccountId;

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
