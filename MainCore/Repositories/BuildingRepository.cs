using Humanizer;
using MainCore.Common.Models;
using MainCore.UI.Models.Output;
using System.Text;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class BuildingRepository : IBuildingRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public BuildingRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public bool IsRallyPointExists(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var isExists = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == BuildingEnums.RallyPoint)
                .Where(x => x.Level > 0)
                .Any();
            return isExists;
        }

        public List<ListBoxItem> GetItems(VillageId villageId)
        {
            var items = new GetBuildings().Execute(villageId).Select(x => ToListBoxItem(x)).ToList();
            return items;
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

        public void UpdateWall(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var wallBuilding = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == 40)
                .FirstOrDefault();
            if (wallBuilding is null) return;
            var tribe = (TribeEnums)context.VillagesSetting
                       .Where(x => x.VillageId == villageId.Value)
                       .Where(x => x.Setting == VillageSettingEnums.Tribe)
                       .Select(x => x.Value)
                       .FirstOrDefault();
            var wall = tribe.GetWall();
            if (wallBuilding.Type == wall) return;
            wallBuilding.Type = wall;
            context.Update(wallBuilding);
            context.SaveChanges();
        }

        public List<BuildingEnums> GetNormalBuilding(VillageId villageId, BuildingId buildingId)
        {
            var buildingItems = new GetBuildings().Execute(villageId);
            var type = buildingItems
                .Where(x => x.Id == buildingId)
                .Select(x => x.Type)
                .FirstOrDefault();
            if (type != BuildingEnums.Site) return new() { type };
            using var context = _contextFactory.CreateDbContext();

            var buildings = buildingItems
                .Select(x => x.Type)
                .Where(x => !MultipleBuildings.Contains(x))
                .Distinct()
                .ToList();

            return AvailableBuildings.Where(x => !buildings.Contains(x)).ToList();
        }

        private static readonly List<BuildingEnums> IgnoreBuildings = new()
        {
            BuildingEnums.Site,
            BuildingEnums.Blacksmith,
            BuildingEnums.GreatBarracks,
            BuildingEnums.GreatStable,
            BuildingEnums.CityWall,
            BuildingEnums.EarthWall,
            BuildingEnums.Palisade,
            BuildingEnums.GreatWarehouse,
            BuildingEnums.GreatGranary,
            BuildingEnums.WW,
            BuildingEnums.StoneWall,
            BuildingEnums.MakeshiftWall,
        };

        private static readonly List<BuildingEnums> MultipleBuildings = new()
        {
            BuildingEnums.Warehouse,
            BuildingEnums.Granary,
            BuildingEnums.Trapper,
            BuildingEnums.Cranny,
        };

        private static readonly IEnumerable<BuildingEnums> AvailableBuildings = Enum.GetValues(typeof(BuildingEnums))
            .Cast<BuildingEnums>()
            .Where(x => !IgnoreBuildings.Contains(x));
    }
}