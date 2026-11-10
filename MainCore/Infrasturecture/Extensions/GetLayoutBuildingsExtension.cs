using System.Text.Json;

namespace MainCore.Infrasturecture.Extensions
{
    public static class GetLayoutBuildingsExtension
    {
        extension(AppDbContext context)
        {
            public List<BuildingItem> GetLayoutBuildings(VillageId villageId, bool ignoreJobBuilding = false)
            {
                var buildingsByLocation = context.Buildings
                    .AsNoTracking()
                    .Where(b => b.VillageId == villageId.Value)
                    .OrderBy(b => b.Location)
                    .Select(b => new BuildingItem()
                    {
                        Id = new(b.Id),
                        Location = b.Location,
                        Type = b.Type,
                        CurrentLevel = b.Level
                    })
                    .ToDictionary(b => b.Location);

                var activeUpgrades = context.QueueBuildings
                    .AsNoTracking()
                    .Where(qb => qb.VillageId == villageId.Value)
                    .GroupBy(qb => qb.Location)
                    .ToList();

                foreach (var upgradeGroup in activeUpgrades)
                {
                    if (!buildingsByLocation.TryGetValue(upgradeGroup.Key, out var building))
                        continue;

                    var highestUpgrade = upgradeGroup.MaxBy(qb => qb.Level);
                    if (highestUpgrade is null)
                        continue;

                    if (building.Type == BuildingEnums.Site)
                        building.Type = highestUpgrade.Type;

                    if (building.QueueLevel < highestUpgrade.Level)
                        building.QueueLevel = highestUpgrade.Level;
                }

                if (!ignoreJobBuilding)
                {
                    var plannedNormalBuilds = context.Jobs
                        .AsNoTracking()
                        .Where(j => j.VillageId == villageId.Value && j.Type == JobTypeEnums.NormalBuild)
                        .Select(j => j.Content)
                        .AsEnumerable()
                        .Select(content => JsonSerializer.Deserialize<NormalBuildPlan>(content)!)
                        .GroupBy(plan => plan.Location);

                    foreach (var planGroup in plannedNormalBuilds)
                    {
                        if (!buildingsByLocation.TryGetValue(planGroup.Key, out var building))
                            continue;

                        var highestPlannedJob = planGroup.MaxBy(plan => plan.Level);
                        if (highestPlannedJob is null)
                            continue;

                        if (building.Type == BuildingEnums.Site)
                            building.Type = highestPlannedJob.Type;

                        if (building.JobLevel < highestPlannedJob.Level)
                            building.JobLevel = highestPlannedJob.Level;
                    }

                    var plannedResourceBuilds = context.Jobs
                        .AsNoTracking()
                        .Where(j => j.VillageId == villageId.Value && j.Type == JobTypeEnums.ResourceBuild)
                        .Select(j => j.Content)
                        .AsEnumerable()
                        .Select(content => JsonSerializer.Deserialize<ResourceBuildPlan>(content)!)
                        .GroupBy(plan => plan.Plan);

                    var resourceFields = buildingsByLocation.Values
                        .Where(b => b.Type.IsResourceField())
                        .ToList();

                    foreach (var planGroup in plannedResourceBuilds)
                    {
                        var highestPlannedJob = planGroup.MaxBy(plan => plan.Level);
                        if (highestPlannedJob is null)
                            continue;

                        if (planGroup.Key == ResourcePlanEnums.AllResources)
                        {
                            resourceFields.ForEach(b => b.JobLevel = Math.Max(b.JobLevel, highestPlannedJob.Level));
                            continue;
                        }

                        if (planGroup.Key == ResourcePlanEnums.ExcludeCrop)
                        {
                            resourceFields
                                .Where(b => b.Type != BuildingEnums.Cropland)
                                .ToList()
                                .ForEach(b => b.JobLevel = Math.Max(b.JobLevel, highestPlannedJob.Level));
                            continue;
                        }

                        if (planGroup.Key == ResourcePlanEnums.OnlyCrop)
                        {
                            resourceFields
                                .Where(b => b.Type == BuildingEnums.Cropland)
                                .ToList()
                                .ForEach(b => b.JobLevel = Math.Max(b.JobLevel, highestPlannedJob.Level));
                        }
                    }
                }

                return [.. buildingsByLocation.Values.OrderBy(b => b.Location)];
            }
        }
    }
}