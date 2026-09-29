using System.Text.Json;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public sealed partial class UpgradeCommand(AddJobCommand.Handler addJobCommand, IDbContextFactory<AppDbContext> contextFactory)
    {
        public sealed record Command(VillageId VillageId, int Location, bool IsMaxLevel) : IVillageCommand;

        private async ValueTask HandleAsync(Command command)
        {
            var (villageId, location, isMaxLevel) = command;
            var building = GetBuilding(villageId, location);

            if (building is null) return;
            if (building.Type == BuildingEnums.Site) return;

            var level = 0;

            if (isMaxLevel)
            {
                level = building.Type.GetMaxLevel();
            }
            else
            {
                level = building.Level + 1;
            }

            var plan = new NormalBuildPlan()
            {
                Location = location,
                Type = building.Type,
                Level = level,
            };

            var job = new JobDto()
            {
                Position = 0,
                Type = JobTypeEnums.NormalBuild,
                Content = JsonSerializer.Serialize(plan),
            };

            await addJobCommand.HandleAsync(new(villageId, job));
        }

        private BuildingItem? GetBuilding(VillageId villageId, int location)
        {
            using var context = contextFactory.CreateDbContext();
            var buildings = context.GetLayoutBuildings(villageId);
            var building = buildings.Find(x => x.Location == location);
            return building;
        }
    }
}