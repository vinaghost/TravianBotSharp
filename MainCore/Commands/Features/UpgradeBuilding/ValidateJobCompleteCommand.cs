using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class ValidateJobCompleteCommand
    {
        public sealed record Command(VillageId VillageId, JobDto job) : IVillageCommand;

        private static async ValueTask<bool> HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
        )
        {
            await Task.CompletedTask;
            var (villageId, job) = command;
            if (job.Type == JobTypeEnums.ResourceBuild) return false;

            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;

            var queueBuilding = context.QueueBuildings
                .Where(x => x.Location == plan.Location)
                .OrderByDescending(x => x.Level)
                .Select(x => x.Level)
                .FirstOrDefault();

            if (queueBuilding >= plan.Level) return true;

            var villageBuilding = context.Buildings
                .Where(x => x.Location == plan.Location)
                .Select(x => x.Level)
                .FirstOrDefault();
            if (villageBuilding >= plan.Level) return true;

            return false;
        }
    }
}