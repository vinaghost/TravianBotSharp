using MainCore.Constraints;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class DeleteJobByVillageIdCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context, JobUpdated.Handler jobUpdated,
            CancellationToken cancellationToken
            )
        {
            var (accountId, villageId) = command;


            context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .ExecuteDelete();

            await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
        }
    }
}