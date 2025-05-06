using MainCore.Constraints;
using System.Text.Json;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class ExportCommand
    {
        public sealed record Command(VillageId VillageId, string Path) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            var (villageId, path) = command;
            

            var jobs = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .OrderBy(x => x.Position)
                .ToDto()
                .ToList();
            jobs.ForEach(job => job.Id = JobId.Empty);
            var jsonString = JsonSerializer.Serialize(jobs);
            await File.WriteAllTextAsync(path, jsonString);
        }
    }
}