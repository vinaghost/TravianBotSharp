using MainCore.Constraints;

namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class DeleteJobByIdCommand
    {
        public sealed record Command(VillageId VillageId, JobId JobId, string Reason = "") : IVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            ILogger logger,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var (villageId, jobId, reason) = command;

            var job = context.Jobs
                .Where(x => x.Id == jobId.Value)
                .Select(x => new
                {
                    x.VillageId,
                    x.Position
                })
                .FirstOrDefault();

            if (job is null) return;

            logger.Information("Delete job {JobId} in village {VillageId}: {Reason}", jobId.Value, villageId.Value, reason);

            context.Jobs
                .Where(x => x.Id == jobId.Value)
                .ExecuteDelete();

            context.Jobs
                .Where(x => x.VillageId == job.VillageId)
                .Where(x => x.Position > job.Position)
                .ExecuteUpdate(x => x.SetProperty(x => x.Position, x => x.Position - 1));
        }
    }
}