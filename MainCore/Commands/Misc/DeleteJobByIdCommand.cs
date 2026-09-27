namespace MainCore.Commands.Misc
{
    [Handler]
    public sealed partial class DeleteJobByIdCommand(IDbContextFactory<AppDbContext> contextFactory)
    {
        public sealed record Command(JobId JobId) : ICommand;

        private async ValueTask HandleAsync(Command command)
        {
            await Task.CompletedTask;

            using var context = contextFactory.CreateDbContext();
            var jobId = command.JobId;

            var job = context.Jobs
                .Where(x => x.Id == jobId.Value)
                .Select(x => new
                {
                    x.VillageId,
                    x.Position
                })
                .FirstOrDefault();

            if (job is null) return;

            context.Jobs
                .Where(x => x.Id == jobId.Value)
                .ExecuteDelete();

            context.Jobs
                .Where(x => x.VillageId == job.VillageId)
                .Where(x => x.Position > job.Position)
                .ExecuteUpdate(x => x.SetProperty(x => x.Position, x => x.Position - 1));
        }
    }
}