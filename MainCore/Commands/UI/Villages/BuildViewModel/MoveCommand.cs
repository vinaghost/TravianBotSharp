namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class MoveCommand
    {
        public sealed record Command(VillageId VillageId, JobId jobId, MoveEnums Move) : IVillageCommand;

        private static async ValueTask<int> HandleAsync(
            Command command,
            AppDbContext context
            )
        {
            await Task.CompletedTask;
            var (villageId, jobId, move) = command;

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

            return job.Position;
        }
    }
}