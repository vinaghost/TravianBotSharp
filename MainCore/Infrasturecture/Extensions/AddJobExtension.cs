using System;
using System.Collections.Generic;
using System.Text;

namespace MainCore.Infrasturecture.Extensions
{
    public static class AddJobExtension
    {
        public static void AddJob(this AppDbContext context, VillageId villageId, JobDto job, bool toTop = false)
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