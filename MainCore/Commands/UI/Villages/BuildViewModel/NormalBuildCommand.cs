using MainCore.UI.Models.Input;
using System.Text.Json;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public sealed partial class NormalBuildCommand(
        AddJobCommand.Handler addJobCommand,
        IDbContextFactory<AppDbContext> contextFactory)
    {
        public sealed record Command(VillageId VillageId, NormalBuildPlan plan) : IVillageCommand;

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var (villageId, plan) = command;

            var buildings = GetBuildings(villageId);
            var building = buildings.Find(x => x.Location == plan.Location);

            if (building is null)
            {
                var result = CheckRequirements(plan, buildings);
                if (result.IsFailed) return result;
                ValidateLocation(plan, buildings);
            }
            var job = new JobDto()
            {
                Position = 0,
                Type = JobTypeEnums.NormalBuild,
                Content = JsonSerializer.Serialize(plan),
            };
            await addJobCommand.HandleAsync(new(villageId, job));
            return Result.Ok();
        }

        private List<BuildingItem> GetBuildings(VillageId villageId)
        {
            using var context = contextFactory.CreateDbContext();
            return context.GetLayoutBuildings(villageId);
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

        private static void ValidateLocation(NormalBuildPlan plan, List<BuildingItem> buildings)
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