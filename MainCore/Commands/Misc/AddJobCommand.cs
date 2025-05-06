using MainCore.Constraints;
using System.Text.Json;

namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class AddJobCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, Job Job, bool Top = false) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context, JobUpdated.Handler jobUpdated,
            CancellationToken cancellationToken
            )
        {
            var (accountId, villageId, job, top) = command;


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

            context.Add(job);
            context.SaveChanges();

            await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
        }

        public static Job ToJob(this NormalBuildPlan plan, VillageId villageId)
        {
            return new Job()
            {
                Position = 0,
                VillageId = villageId.Value,
                Type = JobTypeEnums.NormalBuild,
                Content = JsonSerializer.Serialize(plan),
            };
        }

        public static Job ToJob(this ResourceBuildPlan plan, VillageId villageId)
        {
            return new Job()
            {
                Position = 0,
                VillageId = villageId.Value,
                Type = JobTypeEnums.ResourceBuild,
                Content = JsonSerializer.Serialize(plan),
            };
        }
    }
}