using MainCore.Constraints;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class UpgradeCommand
    {
        public sealed record Command(VillageId VillageId, int Location, bool IsMaxLevel) : IVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AddJobCommand.Handler addJobCommand, GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery,
            CancellationToken cancellationToken
            )
        {
            var (villageId, location, isMaxLevel) = command;
            var buildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId));
            var building = buildings.Find(x => x.Location == location);

            if (building is null) return;
            if (building.Type == BuildingEnums.Site) return;

            var level = 0;

            // Fix: Use the maximum of current, queue, and job levels to avoid off-by-one error
            var currentLevel = Math.Max(Math.Max(building.CurrentLevel, building.QueueLevel), building.JobLevel);

            if (isMaxLevel)
            {
                level = building.Type.GetMaxLevel();
            }
            else
            {
                level = currentLevel + 1;
            }

            var plan = new NormalBuildPlan()
            {
                Location = location,
                Type = building.Type,
                Level = level,
            };

            await addJobCommand.HandleAsync(new(villageId, plan.ToJob()));
        }
    }
}