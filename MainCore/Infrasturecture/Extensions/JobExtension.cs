using System.Text.Json;

using MainCore.UI.Models.Input;

namespace MainCore.Infrasturecture.Extensions
{
    public static class JobExtension
    {
        extension(AppDbContext context)
        {
            public void AddJob(VillageId villageId, ResourceBuildInput resourceBuildInput, bool toTop = false)
            {
                var (type, level) = resourceBuildInput.Get();
                var plan = new ResourceBuildPlan()
                {
                    Plan = type,
                    Level = level,
                };
                var job = new JobDto()
                {
                    Position = 0,
                    Type = JobTypeEnums.ResourceBuild,
                    Content = JsonSerializer.Serialize(plan),
                };

                context.AddJob(villageId, job, toTop);
            }

            public void AddJob(VillageId villageId, NormalBuildPlan plan, bool toTop = false)
            {
                var job = new JobDto()
                {
                    Position = 0,
                    Type = JobTypeEnums.NormalBuild,
                    Content = JsonSerializer.Serialize(plan),
                };

                context.AddJob(villageId, job, toTop);
            }

            private void AddJob(VillageId villageId, JobDto job, bool toTop = false)
            {
                if (toTop)
                {
                    context.Jobs
                       .Where(x => x.VillageId == villageId.Value)
                       .ExecuteUpdate(s => s.SetProperty(x => x.Position, x => x.Position + 1));
                    context.ChangeTracker.Clear();

                    job.Position = 0;
                }
                else
                {
                    var maxPosition = context.Jobs
                        .Where(x => x.VillageId == villageId.Value)
                        .Select(x => (int?)x.Position)
                        .Max();

                    job.Position = (maxPosition ?? -1) + 1;
                }

                context.Add(job.ToEntity(villageId));
                context.SaveChanges();
            }
            public void DeleteJobById(JobId jobId)
            {
                var targetJobMetadata = context.Jobs
                   .Where(x => x.Id == jobId.Value)
                   .Select(x => new
                   {
                       x.VillageId,
                       x.Position
                   })
                   .FirstOrDefault();

                if (targetJobMetadata is null) return;

                context.Jobs
                    .Where(x => x.Id == jobId.Value)
                    .ExecuteDelete();

                context.Jobs
                    .Where(x => x.VillageId == targetJobMetadata.VillageId)
                    .Where(x => x.Position > targetJobMetadata.Position)
                    .ExecuteUpdate(s => s.SetProperty(x => x.Position, x => x.Position - 1));

                context.ChangeTracker.Clear();
            }
        }
    }
}