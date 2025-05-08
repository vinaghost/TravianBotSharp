using MainCore.Constraints;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class ImportCommand
    {
        public sealed record Command(VillageId VillageId, List<JobDto> Jobs) : IVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var (villageId, inputJobs) = command;

            var count = context.Jobs
               .Where(x => x.VillageId == villageId.Value)
               .Count();

            var jobs = inputJobs
                .Select((job, index) => new Job()
                {
                    Position = count + index,
                    VillageId = villageId.Value,
                    Type = job.Type,
                    Content = job.Content,
                })
                .ToList();

            context.AddRange(jobs);
            context.SaveChanges();
        }
    }
}