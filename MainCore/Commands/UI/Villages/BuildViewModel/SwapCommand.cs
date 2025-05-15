using MainCore.Constraints;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class SwapCommand
    {
        public sealed record Command(VillageId VillageId, JobId jobId, MoveEnums Move) : IVillageCommand;

        private static async ValueTask<int> HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var (villageId, jobId, move) = command;

            var job = context.Jobs
                .Where(x => x.Id == jobId.Value)
                .FirstOrDefault();

            if (job is null) return -1;

            var currentPosition = job.Position;
            Job? targetJob;

            switch (move)
            {
                case MoveEnums.Up:
                    if (currentPosition == 0) return currentPosition;
                    targetJob = context.Jobs
                        .Where(x => x.VillageId == job.VillageId)
                        .Where(x => x.Position == currentPosition - 1)
                        .FirstOrDefault();
                    break;

                case MoveEnums.Down:
                    var count = context.Jobs
                        .Where(x => x.VillageId == job.VillageId)
                        .Count();
                    if (currentPosition == count - 1) return currentPosition;
                    targetJob = context.Jobs
                        .Where(x => x.VillageId == job.VillageId)
                        .Where(x => x.Position == currentPosition + 1)
                        .FirstOrDefault();
                    break;

                default:
                    return currentPosition;
            }
            if (targetJob is null) return currentPosition;

            (targetJob.Position, job.Position) = (job.Position, targetJob.Position);

            context.Update(job);
            context.Update(targetJob);
            context.SaveChanges();

            return job.Position;
        }
    }
}