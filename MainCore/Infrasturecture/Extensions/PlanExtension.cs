using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore.Internal;

namespace MainCore.Infrasturecture.Extensions
{
    public static class PlanExtension
    {
        public static NormalBuildPlan? ExtractResourceBuildPlan(this AppDbContext context, VillageId villageId, ResourceBuildPlan plan)
        {
            var layoutBuildings = context.GetLayoutBuildings(villageId, ignoreJobBuilding: true);

            var candidateFields = plan.Plan switch
            {
                ResourcePlanEnums.ExcludeCrop => layoutBuildings.Where(x => x.Type == BuildingEnums.Woodcutter || x.Type == BuildingEnums.ClayPit || x.Type == BuildingEnums.IronMine),
                ResourcePlanEnums.OnlyCrop => layoutBuildings.Where(x => x.Type == BuildingEnums.Cropland),
                _ => layoutBuildings.Where(x => x.Type.IsResourceField())
            };

            var validFields = candidateFields.Where(x => x.Level < plan.Level).ToList();
            if (validFields.Count == 0) return null;

            var minLevel = validFields.Min(x => x.Level);
            var lowestFields = validFields.Where(x => x.Level == minLevel).ToList();

            var chosenOne = lowestFields[Random.Shared.Next(lowestFields.Count)];

            return new NormalBuildPlan()
            {
                Type = chosenOne.Type,
                Level = chosenOne.Level + 1,
                Location = chosenOne.Location,
            };
        }

        public static bool IsPlanDone(this AppDbContext context, VillageId villageId, NormalBuildPlan plan)
        {
            var building = context.Buildings
                .AsNoTracking()
                .FirstOrDefault(x => x.VillageId == villageId.Value && x.Location == plan.Location);

            if (building is null || building.Type != plan.Type)
                return false;

            if (building.Level >= plan.Level)
                return true;

            var maxQueueLevel = context.QueueBuildings
                .AsNoTracking()
                .Where(x => x.VillageId == villageId.Value && x.Location == plan.Location)
                .Select(x => x.Level)
                .DefaultIfEmpty(0)
                .Max();

            return maxQueueLevel >= plan.Level;
        }
    }
}