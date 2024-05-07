using MainCore.Common.Models;
using System.Text.Json;

namespace MainCore.Commands.Queries
{
    public class GetBuildings
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public GetBuildings(IDbContextFactory<AppDbContext> contextFactory = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        }

        public List<BuildingItem> Execute(VillageId villageId, bool ignoreJobBuilding = false)
        {
            using var context = _contextFactory.CreateDbContext();
            var villageBuildings = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .OrderBy(x => x.Location)
                .Select(x => new BuildingItem()
                {
                    Id = new(x.Id),
                    Location = x.Location,
                    Type = x.Type,
                    CurrentLevel = x.Level
                })
                .ToList();

            var queueBuildings = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .GroupBy(x => x.Location)
                .ToList();

            foreach (var queueBuilding in queueBuildings)
            {
                var building = villageBuildings.FirstOrDefault(x => x.Location == queueBuilding.Key);
                if (building is null) continue;
                var queue = queueBuilding.MaxBy(x => x.Level);
                if (queue is null) continue;
                if (building.Type == BuildingEnums.Site) building.Type = queue.Type;
                building.QueueLevel = queue.Level;
                if (building.QueueLevel < queue.Level) building.JobLevel = queue.Level;
            }
            if (!ignoreJobBuilding)
            {
                var jobBuildings = context.Jobs
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Type == JobTypeEnums.NormalBuild)
                    .Select(x => x.Content)
                    .AsEnumerable()
                    .Select(x => JsonSerializer.Deserialize<NormalBuildPlan>(x))
                    .GroupBy(x => x.Location);

                foreach (var jobBuilding in jobBuildings)
                {
                    var building = villageBuildings.FirstOrDefault(x => x.Location == jobBuilding.Key);
                    if (building is null) continue;
                    var job = jobBuilding.MaxBy(x => x.Level);
                    if (job is null) continue;
                    if (building.Type == BuildingEnums.Site) building.Type = job.Type;
                    if (building.JobLevel < job.Level) building.JobLevel = job.Level;
                }

                var resourceJobs = context.Jobs
                   .Where(x => x.VillageId == villageId.Value)
                   .Where(x => x.Type == JobTypeEnums.ResourceBuild)
                   .Select(x => x.Content)
                   .AsEnumerable()
                   .Select(x => JsonSerializer.Deserialize<ResourceBuildPlan>(x))
                   .GroupBy(x => x.Plan);

                var fields = villageBuildings.Where(x => x.Type.IsResourceField()).ToList();

                foreach (var jobBuilding in resourceJobs)
                {
                    var job = jobBuilding.FirstOrDefault();
                    if (job is null) continue;
                    if (jobBuilding.Key == ResourcePlanEnums.AllResources)
                    {
                        fields
                            .ForEach(x => x.JobLevel = x.JobLevel < job.Level ? job.Level : x.JobLevel);
                        continue;
                    }
                    if (jobBuilding.Key == ResourcePlanEnums.ExcludeCrop)
                    {
                        fields
                            .Where(x => x.Type != BuildingEnums.Cropland)
                            .ToList()
                            .ForEach(x => x.JobLevel = x.JobLevel < job.Level ? job.Level : x.JobLevel);
                    }
                    if (jobBuilding.Key == ResourcePlanEnums.OnlyCrop)
                    {
                        fields
                            .Where(x => x.Type == BuildingEnums.Cropland)
                            .ToList()
                            .ForEach(x => x.JobLevel = x.JobLevel < job.Level ? job.Level : x.JobLevel);
                    }
                }
            }
            return villageBuildings;
        }
    }
}