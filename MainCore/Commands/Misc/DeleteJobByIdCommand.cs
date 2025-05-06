using MainCore.Constraints;

namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class DeleteJobByIdCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, JobId JobId) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context, JobUpdated.Handler jobUpdated,
            CancellationToken cancellationToken
            )
        {
            var (accountId, villageId, jobId) = command;
            

            var job = context.Jobs
                .Where(x => x.Id == jobId.Value)
                .Select(x => new
                {
                    x.VillageId,
                    x.Position
                })
                .FirstOrDefault();

            if (job is null) return;

            context.Jobs
                .Where(x => x.Id == jobId.Value)
                .ExecuteDelete();

            context.Jobs
                .Where(x => x.VillageId == job.VillageId)
                .Where(x => x.Position > job.Position)
                .ExecuteUpdate(x => x.SetProperty(x => x.Position, x => x.Position - 1));

            await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
        }
    }
}