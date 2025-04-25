namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class DeleteJobByVillageIdCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : ICustomCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IDbContextFactory<AppDbContext> contextFactory, JobUpdated.Handler jobUpdated,
            CancellationToken cancellationToken
            )
        {
            var (accountId, villageId) = command;
            using var context = await contextFactory.CreateDbContextAsync();

            context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .ExecuteDelete();

            await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
        }
    }
}