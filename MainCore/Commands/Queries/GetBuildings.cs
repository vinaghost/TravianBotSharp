using Humanizer;
using MainCore.Commands.Abstract;
using MainCore.Common.Models;
using MainCore.UI.Models.Output;
using System.Text;
using System.Text.Json;

namespace MainCore.Commands.Queries
{
    [RegisterSingleton<GetBuildings>]
    public class GetBuildings(IDbContextFactory<AppDbContext> contextFactory) : QueryBase(contextFactory)
    {
        private static List<BuildingItem> Layout(AppDbContext context, VillageId villageId, bool ignoreJobBuilding = false)
        {
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
                var building = villageBuildings.Find(x => x.Location == queueBuilding.Key);
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
                    var building = villageBuildings.Find(x => x.Location == jobBuilding.Key);
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

        public List<ListBoxItem> LayoutItems(VillageId villageId, bool ignoreJobBuilding = false)
        {
            using var context = _contextFactory.CreateDbContext();
            var buildings = Layout(context, villageId, ignoreJobBuilding);
            return buildings.Select(ToListBoxItem).ToList();
        }

        public List<BuildingItem> Layout(VillageId villageId, bool ignoreJobBuilding = false)
        {
            using var context = _contextFactory.CreateDbContext();
            return Layout(context, villageId, ignoreJobBuilding);
        }

        public List<ListBoxItem> QueueItems(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            var queue = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .AsEnumerable()
                .Select(x => new ListBoxItem()
                {
                    Id = x.Id,
                    Content = $"{x.Type.Humanize()} to level {x.Level} complete at {x.CompleteTime}",
                })
                .ToList();

            var tribe = (TribeEnums)context.VillagesSetting
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Setting == VillageSettingEnums.Tribe)
                .Select(x => x.Value)
                .FirstOrDefault();

            var count = 2;
            if (tribe == TribeEnums.Romans) count = 3;
            queue.AddRange(Enumerable.Range(0, count - queue.Count).Select(x => new ListBoxItem()));

            return queue;
        }

        private static readonly List<BuildingEnums> MultipleBuildings =
        [
            BuildingEnums.Warehouse,
            BuildingEnums.Granary,
            BuildingEnums.Trapper,
            BuildingEnums.Cranny,
        ];

        private static readonly List<BuildingEnums> IgnoreBuildings =
        [
            BuildingEnums.Site,
            BuildingEnums.Blacksmith,
            BuildingEnums.CityWall,
            BuildingEnums.EarthWall,
            BuildingEnums.Palisade,
            BuildingEnums.WW,
            BuildingEnums.StoneWall,
            BuildingEnums.MakeshiftWall,
        ];

        private static readonly IEnumerable<BuildingEnums> AvailableBuildings = Enum.GetValues(typeof(BuildingEnums))
            .Cast<BuildingEnums>()
            .Where(x => !IgnoreBuildings.Contains(x));

        public List<BuildingEnums> NormalBuilds(VillageId villageId, BuildingId buildingId)
        {
            using var context = _contextFactory.CreateDbContext();

            var buildingItems = Layout(context, villageId);

            var type = buildingItems
                .Where(x => x.Id == buildingId)
                .Select(x => x.Type)
                .FirstOrDefault();
            if (type != BuildingEnums.Site) return [type];

            var buildings = buildingItems
                .Select(x => x.Type)
                .Where(x => !MultipleBuildings.Contains(x))
                .Distinct()
                .ToList();

            return AvailableBuildings.Where(x => !buildings.Contains(x)).ToList();
        }

        private static ListBoxItem ToListBoxItem(BuildingItem building)
        {
            const string arrow = " -> ";
            var sb = new StringBuilder();
            sb.Append(building.CurrentLevel);
            if (building.QueueLevel != 0)
            {
                var content = $"{arrow}({building.QueueLevel})";
                sb.Append(content);
            }
            if (building.JobLevel != 0)
            {
                var content = $"{arrow}[{building.JobLevel}]";
                sb.Append(content);
            }

            var item = new ListBoxItem()
            {
                Id = building.Id.Value,
                Content = $"[{building.Location}] {building.Type.Humanize()} | lvl {sb}",
                Color = building.Type.GetColor(),
            };
            return item;
        }
    }
}