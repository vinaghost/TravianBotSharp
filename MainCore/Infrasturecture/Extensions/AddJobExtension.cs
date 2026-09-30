using MainCore.UI.Models.Input;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace MainCore.Infrasturecture.Extensions
{
    public static class AddJobExtension
    {
        public static void AddJob(this AppDbContext context, VillageId villageId, ResourceBuildInput resourceBuildInput, bool toTop = false)
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

        public static void AddJob(this AppDbContext context, VillageId villageId, NormalBuildPlan plan, bool toTop = false)
        {
            var job = new JobDto()
            {
                Position = 0,
                Type = JobTypeEnums.NormalBuild,
                Content = JsonSerializer.Serialize(plan),
            };

            context.AddJob(villageId, job, toTop);
        }

        private static void AddJob(this AppDbContext context, VillageId villageId, JobDto job, bool toTop = false)
        {
            if (toTop)
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