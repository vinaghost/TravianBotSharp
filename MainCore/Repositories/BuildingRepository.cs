using Humanizer;
using MainCore.Common.Enums;
using MainCore.Common.Extensions;
using MainCore.Common.Models;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Infrasturecture.Persistence;
using MainCore.UI.Models.Output;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class BuildingRepository : IBuildingRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        private static readonly Dictionary<ResourcePlanEnums, List<BuildingEnums>> _fieldList = new()
        {
            {ResourcePlanEnums.AllResources, new(){
                BuildingEnums.Woodcutter,
                BuildingEnums.ClayPit,
                BuildingEnums.IronMine,
                BuildingEnums.Cropland,}},
            {ResourcePlanEnums.ExcludeCrop, new() {
                BuildingEnums.Woodcutter,
                BuildingEnums.ClayPit,
                BuildingEnums.IronMine,}},
            {ResourcePlanEnums.OnlyCrop, new() {
                BuildingEnums.Cropland,}},
        };

        public BuildingRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public BuildingDto GetBuilding(VillageId villageId, int location)
        {
            using var context = _contextFactory.CreateDbContext();
            var building = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == location)
                .ToDto()
                .FirstOrDefault();
            return building;
        }

        public int CountQueueBuilding(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .Count();
            return count;
        }

        public int CountResourceQueueBuilding(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var resourceTypes = new List<BuildingEnums>()
            {
                BuildingEnums.Woodcutter,
                BuildingEnums.ClayPit,
                BuildingEnums.IronMine,
                BuildingEnums.Cropland
            };

            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => resourceTypes.Contains(x.Type))
                .Count();
            return count;
        }

        public int GetBuildingLocation(VillageId villageId, BuildingEnums building)
        {
            using var context = _contextFactory.CreateDbContext();
            var location = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == building)
                .Select(x => x.Location)
                .FirstOrDefault();
            return location;
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

        public bool EmptySite(VillageId villageId, int location)
        {
            using var context = _contextFactory.CreateDbContext();
            bool isEmptySite = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == location)
                .Where(x => x.Type == BuildingEnums.Site || x.Level == -1)
                .Any();

            return isEmptySite;
        }

        public Building GetCropland(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var building = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == BuildingEnums.Cropland)
                .OrderBy(x => x.Level)
                .FirstOrDefault();
            return building;
        }

        public NormalBuildPlan GetNormalBuildPlan(VillageId villageId, ResourceBuildPlan plan)
        {
            var resourceTypes = _fieldList[plan.Plan];

            var buildings = GetBuildingItems(villageId, true);

            buildings = buildings
                .Where(x => resourceTypes.Contains(x.Type))
                .Where(x => x.Level < plan.Level)
                .ToList();

            if (!buildings.Any()) return null;

            var chosenOne = buildings
                .OrderBy(x => x.Id.Value + Random.Shared.Next())
                .OrderBy(x => x.Level)
                .FirstOrDefault();

            var normalBuildPlan = new NormalBuildPlan()
            {
                Type = chosenOne.Type,
                Level = chosenOne.Level + 1,
                Location = chosenOne.Location,
            };
            return normalBuildPlan;
        }

        public List<ListBoxItem> GetItems(VillageId villageId)
        {
            var items = GetBuildingItems(villageId).Select(x => ToListBoxItem(x)).ToList();
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

        public List<BuildingItem> GetBuildings(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var villageBuildings = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Select(x => new BuildingItem()
                {
                    Id = new(x.Id),
                    Location = x.Location,
                    Type = x.Type,
                    CurrentLevel = x.Level
                })
                .ToList();
            return villageBuildings;
        }

        public List<BuildingItem> GetBuildingItems(VillageId villageId, bool ignoreJobBuilding = false)
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
                .AsEnumerable();

            foreach (var queueBuilding in queueBuildings)
            {
                var building = villageBuildings.FirstOrDefault(x => x.Location == queueBuilding.Key);
                if (building is null) continue;
                var queue = queueBuilding.FirstOrDefault();
                if (queue is null) continue;
                if (building.Type == BuildingEnums.Site) building.Type = queue.Type;
                building.QueueLevel = queue.Level;
            }
            if (!ignoreJobBuilding)
            {
                var jobBuildings = context.Jobs
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Type == JobTypeEnums.NormalBuild)
                    .Select(x => x.Content)
                    .AsEnumerable()
                    .AsParallel()
                    .AsOrdered()
                    .Select(x => JsonSerializer.Deserialize<NormalBuildPlan>(x))
                    .GroupBy(x => x.Location);

                foreach (var jobBuilding in jobBuildings)
                {
                    var building = villageBuildings.FirstOrDefault(x => x.Location == jobBuilding.Key);
                    if (building is null) continue;
                    var job = jobBuilding.MaxBy(x => x.Level);
                    if (job is null) continue;
                    if (building.Type == BuildingEnums.Site) building.Type = job.Type;
                    if (building.JobLevel <= job.Level) building.JobLevel = job.Level;
                }

                var resourceJobs = context.Jobs
                   .Where(x => x.VillageId == villageId.Value)
                   .Where(x => x.Type == JobTypeEnums.ResourceBuild)
                   .Select(x => x.Content)
                   .AsEnumerable()
                   .AsParallel()
                   .AsOrdered()
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

        public List<BuildingEnums> GetTrainTroopBuilding(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var settings = new List<VillageSettingEnums>() {
                VillageSettingEnums.BarrackTroop,
                VillageSettingEnums.StableTroop,
                VillageSettingEnums.WorkshopTroop,
            };

            var filterdSettings = context.VillagesSetting
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => settings.Contains(x.Setting))
                .Where(x => x.Value != 0)
                .Select(x => x.Setting)
                .ToList();

            var buildings = new List<BuildingEnums>();

            if (filterdSettings.Contains(VillageSettingEnums.BarrackTroop))
            {
                buildings.Add(BuildingEnums.Barracks);
            }
            if (filterdSettings.Contains(VillageSettingEnums.StableTroop))
            {
                buildings.Add(BuildingEnums.Stable);
            }
            if (filterdSettings.Contains(VillageSettingEnums.WorkshopTroop))
            {
                buildings.Add(BuildingEnums.Workshop);
            }
            return buildings;
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

        public void Update(VillageId villageId, List<BuildingDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();
            var dbBuildings = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .ToList();

            foreach (var dto in dtos)
            {
                if (dto.Location == 40)
                {
                    var tribe = (TribeEnums)context.VillagesSetting
                        .Where(x => x.VillageId == villageId.Value)
                        .Where(x => x.Setting == VillageSettingEnums.Tribe)
                        .Select(x => x.Value)
                        .FirstOrDefault();

                    var wall = tribe.GetWall();
                    dto.Type = wall;
                }
                var dbBuilding = dbBuildings
                    .FirstOrDefault(x => x.Location == dto.Location);
                if (dbBuilding is null)
                {
                    var building = dto.ToEntity(villageId);
                    context.Add(building);
                }
                else
                {
                    dto.To(dbBuilding);
                    context.Update(dbBuilding);
                }
            }
            context.SaveChanges();
        }

        public List<BuildingEnums> GetNormalBuilding(VillageId villageId, BuildingId buildingId)
        {
            var buildingItems = GetBuildingItems(villageId);
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