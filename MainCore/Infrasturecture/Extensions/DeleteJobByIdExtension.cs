namespace MainCore.Infrasturecture.Extensions
{
    public static class DeleteJobByIdExtension
    {
        public static void DeleteJobById(this AppDbContext context, JobId jobId)
        {
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