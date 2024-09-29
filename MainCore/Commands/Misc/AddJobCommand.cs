using MainCore.Commands.Abstract;
using MainCore.Common.Models;
using System.Text.Json;

namespace MainCore.Commands.Misc
{
    public class AddJobCommand(IDbContextFactory<AppDbContext> contextFactory = null) : QueryBase(contextFactory)
    {
        private static readonly Dictionary<Type, JobTypeEnums> _jobTypes = new()
        {
            { typeof(NormalBuildPlan),JobTypeEnums.NormalBuild  },
            { typeof(ResourceBuildPlan),JobTypeEnums.ResourceBuild },
        };

        public void ToTop<T>(VillageId villageId, T content)
        {
            using var context = _contextFactory.CreateDbContext();

            context.Jobs
               .Where(x => x.VillageId == villageId.Value)
               .ExecuteUpdate(x =>
                   x.SetProperty(x => x.Position, x => x.Position + 1));

            var job = new Job()
            {
                Position = 0,
                VillageId = villageId.Value,
                Type = _jobTypes[typeof(T)],
                Content = JsonSerializer.Serialize(content),
            };
            context.Add(job);
            context.SaveChanges();
        }

        public void ToBottom<T>(VillageId villageId, T content)
        {
            using var context = _contextFactory.CreateDbContext();
            var count = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Count();

            var job = new Job()
            {
                Position = count,
                VillageId = villageId.Value,
                Type = _jobTypes[typeof(T)],
                Content = JsonSerializer.Serialize(content),
            };

            context.Add(job);
            context.SaveChanges();
        }
    }
}