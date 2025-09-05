using System.Text.Json;

namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class AddJobCommand
    {
        public sealed record Command(VillageId VillageId, JobDto Job, bool ToTop = false) : IVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context
            )
        {
            await Task.CompletedTask;
            var (villageId, job, top) = command;

            if (top)
            {
                context.Jobs
                   .Where(x => x.VillageId == villageId.Value)
                   .ExecuteUpdate(x =>
                       x.SetProperty(x => x.Position, x => x.Position + 1));
                job.Position = 0;
            }
            else
            {
                var count = context.Jobs
                    .Where(x => x.VillageId == villageId.Value)
                    .Count();

                job.Position = count;
            }

            context.Add(job.ToEntity(villageId));
            context.SaveChanges();
        }

        public static JobDto ToJob(this NormalBuildPlan plan)
        {
            return new JobDto()
            {
                Position = 0,
                Type = JobTypeEnums.NormalBuild,
                Content = JsonSerializer.Serialize(plan),
            };
        }

        public static JobDto ToJob(this ResourceBuildPlan plan)
        {
            return new JobDto()
            {
                Position = 0,
                Type = JobTypeEnums.ResourceBuild,
                Content = JsonSerializer.Serialize(plan),
            };
        }
    }
}
