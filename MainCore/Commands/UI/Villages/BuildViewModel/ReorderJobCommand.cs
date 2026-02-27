namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class ReorderJobCommand
    {
        public sealed record Command(JobId JobId, int TargetPosition) : ICommand;

        private static async ValueTask<int> HandleAsync(
            Command command,
            AppDbContext context
            )
        {
            await Task.CompletedTask;
            var (jobId, targetPosition) = command;

            var job = context.Jobs
                .Where(x => x.Id == jobId.Value)
                .FirstOrDefault();
            if (job is null) return -1;

            var currentPosition = job.Position;
            var count = context.Jobs
                .Where(x => x.VillageId == job.VillageId)
                .Count();
            if (count <= 1) return currentPosition;

            targetPosition = Math.Clamp(targetPosition, 0, count - 1);
            if (targetPosition == currentPosition) return currentPosition;

            if (targetPosition < currentPosition)
            {
                context.Jobs
                    .Where(x => x.VillageId == job.VillageId)
                    .Where(x => x.Position >= targetPosition && x.Position < currentPosition)
                    .ExecuteUpdate(x => x.SetProperty(y => y.Position, y => y.Position + 1));
            }
            else
            {
                context.Jobs
                    .Where(x => x.VillageId == job.VillageId)
                    .Where(x => x.Position <= targetPosition && x.Position > currentPosition)
                    .ExecuteUpdate(x => x.SetProperty(y => y.Position, y => y.Position - 1));
            }

            job.Position = targetPosition;
            context.Update(job);
            context.SaveChanges();

            return job.Position;
        }
    }
}
