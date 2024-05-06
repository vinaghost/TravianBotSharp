using MainCore.Common.Models;
using System.Text.Json;

namespace MainCore.Commands.Misc
{
    public class AddJobToTopCommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public AddJobToTopCommand(IDbContextFactory<AppDbContext> contextFactory = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        }

        private static readonly Dictionary<Type, JobTypeEnums> _jobTypes = new()
        {
            { typeof(NormalBuildPlan),JobTypeEnums.NormalBuild  },
            { typeof(ResourceBuildPlan),JobTypeEnums.ResourceBuild },
        };

        public void Execute<T>(VillageId villageId, T content)
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
    }
}