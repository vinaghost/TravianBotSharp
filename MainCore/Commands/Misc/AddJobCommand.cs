using System.Text.Json;

namespace MainCore.Commands.Misc
{
    [Handler]
    public sealed partial class AddJobCommand(IDbContextFactory<AppDbContext> contextFactory)
    {
        public sealed record Command(VillageId VillageId, JobDto Job, bool ToTop = false) : IVillageCommand;

        private async ValueTask HandleAsync(Command command)
        {
            await Task.CompletedTask;
            var (villageId, job, top) = command;
            using var context = contextFactory.CreateDbContext();
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
                    .Count(x => x.VillageId == villageId.Value);

                job.Position = count;
            }

            context.Add(job.ToEntity(villageId));
            context.SaveChanges();
        }
    }
}