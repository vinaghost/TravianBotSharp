using MainCore.UI.Models.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MainCore.Infrasturecture.Extensions
{
    public static class NormalBuildExtension
    {
        public static Result NormalBuild(this AppDbContext context, VillageId villageId, NormalBuildPlan plan)
        {
            var buildings = context.GetLayoutBuildings(villageId);
            var building = buildings.Find(x => x.Location == plan.Location);

            if (building is null)
            {
                var result = CheckRequirements(plan, buildings);
                if (result.IsFailed) return result;
                plan.ValidateLocation(buildings);
            }
            var job = new JobDto()
            {
                Position = 0,
                Type = JobTypeEnums.NormalBuild,
                Content = JsonSerializer.Serialize(plan),
            };
            context.AddJob(villageId, job);
            return Result.Ok();
        }

        private static Result CheckRequirements(NormalBuildPlan plan, List<BuildingItem> buildings)
        {
            var prerequisiteBuildings = plan.Type.GetPrerequisiteBuildings();
            if (prerequisiteBuildings.Count == 0) return Result.Ok();
            foreach (var prerequisiteBuilding in prerequisiteBuildings)
            {
                var valid = buildings
                    .Where(x => x.Type == prerequisiteBuilding.Type)
                    .Any(x => x.Level >= prerequisiteBuilding.Level);

                if (!valid) return Result.Fail($"Required {prerequisiteBuilding}");
            }
            return Result.Ok();
        }

        private static void ValidateLocation(this NormalBuildPlan plan, List<BuildingItem> buildings)
        {
            if (plan.Type.IsWall())
            {
                plan.Location = 40;
                return;
            }
            if (plan.Type.IsMultipleBuilding())
            {
                var sameTypeBuildings = buildings.Where(x => x.Type == plan.Type);
                if (!sameTypeBuildings.Any()) return;
                if (sameTypeBuildings.Any(x => x.Location == plan.Location)) return;
                var largestLevelBuilding = sameTypeBuildings.MaxBy(x => x.Level)!;
                if (largestLevelBuilding.Level == plan.Type.GetMaxLevel()) return;
                plan.Location = largestLevelBuilding.Location;
                return;
            }

            if (plan.Type.IsResourceField())
            {
                var field = buildings.First(x => x.Location == plan.Location);
                if (plan.Type == field.Type) return;
                plan.Type = field.Type;
                return;
            }

            var building = buildings.Find(x => x.Type == plan.Type);
            if (building is null) return;
            if (plan.Location == building.Location) return;
            plan.Location = building.Location;
        }

        public static NormalBuildPlan ToPlan(NormalBuildInput input, int location)
        {
            var (type, level) = input.Get();
            return new NormalBuildPlan()
            {
                Location = location,
                Type = type,
                Level = level,
            };
        }
    }
}