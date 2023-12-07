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
                .Where(x => x.Type == BuildingEnums.Site)
                .Where(x => x.Location == location)
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
            using var context = _contextFactory.CreateDbContext();

            var resourceTypes = new List<BuildingEnums>();

            switch (plan.Plan)
            {
                case ResourcePlanEnums.AllResources:
                    resourceTypes.AddRange(new[]
                    {
                         BuildingEnums.Woodcutter,
                         BuildingEnums.ClayPit,
                         BuildingEnums.IronMine,
                         BuildingEnums.Cropland,
                    });
                    break;

                case ResourcePlanEnums.ExcludeCrop:
                    resourceTypes.AddRange(new[]
                    {
                         BuildingEnums.Woodcutter,
                         BuildingEnums.ClayPit,
                         BuildingEnums.IronMine,
                    });
                    break;

                case ResourcePlanEnums.OnlyCrop:
                    resourceTypes.AddRange(new[]
                    {
                         BuildingEnums.Cropland,
                    });
                    break;

                default:
                    break;
            }

            var buildings = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => resourceTypes.Contains(x.Type))
                .Where(x => x.Level < plan.Level)
                .ToList();

            var queueBuildings = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => resourceTypes.Contains(x.Type))
                .GroupBy(x => x.Location)
                .Select(x => new
                {
                    Location = x.Key,
                    UpgradingLevel = x.OrderBy(x => x.Level).Count()
                })
                .AsEnumerable();

            foreach (var queueBuilding in queueBuildings)
            {
                var building = buildings.FirstOrDefault(x => x.Location == queueBuilding.Location);
                building.Level += queueBuilding.UpgradingLevel;
            }

            buildings = buildings
                .Where(x => x.Level < plan.Level)
                .ToList();

            if (!buildings.Any()) return null;

            var chosenOne = buildings
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
            using var context = _contextFactory.CreateDbContext();
            var buildings = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .OrderBy(x => x.Location)
                .AsEnumerable()
                .Select(x => new BuildingItemDto()
                {
                    Id = new BuildingId(x.Id),
                    Location = x.Location,
                    Type = x.Type,
                    Level = x.Level,
                })
                .ToList();

            var queueBuildings = context.QueueBuildings.Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .GroupBy(x => x.Location)
                .Select(x => new Building()
                {
                    Location = x.Key,
                    Type = x.OrderBy(x => x.Id).Select(x => x.Type).First(),
                    Level = x.OrderBy(x => x.Id).Select(x => x.Level).Max(),
                })
                .AsEnumerable();

            foreach (var queueBuilding in queueBuildings)
            {
                var villageBuilding = buildings.FirstOrDefault(x => x.Location == queueBuilding.Location);
                villageBuilding.QueueLevel = queueBuilding.Level;
                villageBuilding.Type = queueBuilding.Type;
            }

            var jobBuildings = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == JobTypeEnums.NormalBuild)
                .Select(x => x.Content)
                .AsEnumerable()
                .AsParallel()
                .Select(x => JsonSerializer.Deserialize<NormalBuildPlan>(x))
                .GroupBy(x => x.Location)
                .Select(x => new Building()
                {
                    Location = x.Key,
                    Type = x.OrderBy(x => x.Level).Select(x => x.Type).First(),
                    Level = x.OrderByDescending(x => x.Level).Select(x => x.Level).First(),
                });

            foreach (var jobBuilding in jobBuildings)
            {
                var villageBuilding = buildings.FirstOrDefault(x => x.Location == jobBuilding.Location);
                villageBuilding.JobLevel = jobBuilding.Level;
                villageBuilding.Type = jobBuilding.Type;
            }

            var resourceJobs = context.Jobs
               .Where(x => x.VillageId == villageId.Value)
               .Where(x => x.Type == JobTypeEnums.ResourceBuild)
               .Select(x => x.Content)
               .AsEnumerable()
               .AsParallel()
               .Select(x => JsonSerializer.Deserialize<ResourceBuildPlan>(x))
               .GroupBy(x => x.Plan)
               .Select(x => new ResourceBuildPlan
               {
                   Plan = x.Key,
                   Level = x.OrderByDescending(x => x.Level).Select(x => x.Level).First(),
               })
               .ToDictionary(x => x.Plan, x => x.Level);
            foreach (var building in buildings)
            {
                if (!building.Type.IsResourceField()) continue;
                foreach (var job in resourceJobs)
                {
                    switch (job.Key)
                    {
                        case ResourcePlanEnums.AllResources:
                            building.JobLevel = building.JobLevel < job.Value ? job.Value : building.JobLevel;
                            break;

                        case ResourcePlanEnums.ExcludeCrop:
                            if (building.Type == BuildingEnums.Cropland) break;
                            building.JobLevel = building.JobLevel < job.Value ? job.Value : building.JobLevel;
                            break;

                        case ResourcePlanEnums.OnlyCrop:
                            if (building.Type != BuildingEnums.Cropland) break;
                            building.JobLevel = building.JobLevel < job.Value ? job.Value : building.JobLevel;
                            break;

                        default:
                            break;
                    }
                }
            }

            var items = buildings
                .Select(x => ToListBoxItem(x))
                .ToList();
            return items;
        }

        private static ListBoxItem ToListBoxItem(BuildingItemDto building)
        {
            const string arrow = " -> ";
            var sb = new StringBuilder();
            sb.Append(building.Level);
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

        public List<BuildingItem> GetLevelBuildings(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var villageBuildings = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Select(x => new BuildingItem()
                {
                    Location = x.Location,
                    Type = x.Type,
                    Level = x.Level,
                })
                .AsEnumerable();

            var queueBuildings = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .GroupBy(x => x.Location)
                .Select(x => new BuildingItem()
                {
                    Location = x.Key,
                    Type = x.OrderBy(x => x.Id).Select(x => x.Type).First(),
                    Level = x.OrderByDescending(x => x.Location).Select(x => x.Level).First(),
                })
                .AsEnumerable();

            var jobBuildings = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == JobTypeEnums.NormalBuild)
                .Select(x => x.Content)
                .AsEnumerable()
                .AsParallel()
                .Select(x => JsonSerializer.Deserialize<NormalBuildPlan>(x))
                .GroupBy(x => x.Location)
                .Select(x => new BuildingItem()
                {
                    Location = x.Key,
                    Type = x.OrderBy(x => x.Location).Select(x => x.Type).First(),
                    Level = x.OrderByDescending(x => x.Location).Select(x => x.Level).First(),
                });

            var buildings = new[] { jobBuildings, queueBuildings, villageBuildings }
                .SelectMany(x => x)
                .GroupBy(x => x.Location)
                .Select(x => new BuildingItem()
                {
                    Location = x.Key,
                    Type = x.OrderBy(x => x.Location).Select(x => x.Type).First(),
                    Level = x.OrderByDescending(x => x.Location).Select(x => x.Level).First(),
                })
                .OrderBy(x => x.Location)
                .ToList();

            var resourceJobs = context.Jobs
               .Where(x => x.VillageId == villageId.Value)
               .Where(x => x.Type == JobTypeEnums.ResourceBuild)
               .Select(x => x.Content)
               .AsEnumerable()
               .AsParallel()
               .Select(x => JsonSerializer.Deserialize<ResourceBuildPlan>(x))
               .GroupBy(x => x.Plan)
               .Select(x => new ResourceBuildPlan
               {
                   Plan = x.Key,
                   Level = x.OrderByDescending(x => x.Level).Select(x => x.Level).First(),
               })
               .ToDictionary(x => x.Plan, x => x.Level);
            foreach (var building in buildings)
            {
                if (!building.Type.IsResourceField()) continue;
                foreach (var job in resourceJobs)
                {
                    if (job.Key == ResourcePlanEnums.AllResources)
                    {
                        building.Level = building.Level < job.Value ? job.Value : building.Level;
                        continue;
                    }
                    if (job.Key == ResourcePlanEnums.ExcludeCrop)
                    {
                        if (building.Type == BuildingEnums.Cropland) continue;
                        building.Level = building.Level < job.Value ? job.Value : building.Level;
                        continue;
                    }
                    if (job.Key == ResourcePlanEnums.OnlyCrop)
                    {
                        if (building.Type != BuildingEnums.Cropland) continue;
                        building.Level = building.Level < job.Value ? job.Value : building.Level;
                        continue;
                    }
                }
            }
            return buildings;
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
            using var context = _contextFactory.CreateDbContext();

            var type = context.Buildings
                .Where(x => x.Id == buildingId.Value)
                .Select(x => x.Type)
                .FirstOrDefault();
            if (type != BuildingEnums.Site) return new() { type };

            var villageBuildings = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Select(x => x.Type)
                .Where(x => !MultipleBuildings.Contains(x))
                .Distinct()
                .ToList();
            var jobBuildings = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == JobTypeEnums.NormalBuild)
                .Select(x => x.Content)
                .AsEnumerable()
                .AsParallel()
                .Select(x => JsonSerializer.Deserialize<NormalBuildPlan>(x))
                .Select(x => x.Type)
                .Where(x => !MultipleBuildings.Contains(x))
                .Distinct()
                .ToList();

            var queueBuildings = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Select(x => x.Type)
                .Where(x => !MultipleBuildings.Contains(x))
                .Distinct()
                .ToList();
            var buildings = new[] { villageBuildings, jobBuildings, queueBuildings }
                .SelectMany(x => x)
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