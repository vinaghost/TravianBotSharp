namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class MoveCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, JobId jobId, MoveEnums Move) : ICustomCommand;

        private static async ValueTask<int> HandleAsync(
            Command command,
            IDbContextFactory<AppDbContext> contextFactory, JobUpdated.Handler jobUpdated,
            CancellationToken cancellationToken
            )
        {
            var (accountId, villageId, jobId, move) = command;
            using var context = await contextFactory.CreateDbContextAsync();

            var job = context.Jobs
                .Where(x => x.Id == jobId.Value)
                .FirstOrDefault();

            if (job is null) return -1;

            var currentPosition = job.Position;

            switch (move)
            {
                case MoveEnums.Top:
                    if (currentPosition == 0) return currentPosition;
                    context.Jobs
                        .Where(x => x.VillageId == job.VillageId)
                        .Where(x => x.Position < currentPosition)
                        .ExecuteUpdate(x => x.SetProperty(y => y.Position, y => y.Position + 1));

                    job.Position = 0;
                    break;

                case MoveEnums.Bottom:
                    var count = context.Jobs
                        .Where(x => x.VillageId == job.VillageId)
                        .Count();
                    if (currentPosition == count - 1) return currentPosition;
                    context.Jobs
                        .Where(x => x.VillageId == job.VillageId)
                        .Where(x => x.Position > currentPosition)
                        .ExecuteUpdate(x => x.SetProperty(y => y.Position, y => y.Position - 1));

                    job.Position = count - 1;
                    break;

                default:
                    return currentPosition;
            }

            context.Update(job);
            context.SaveChanges();

            await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
            return job.Position;
        }
    }
}