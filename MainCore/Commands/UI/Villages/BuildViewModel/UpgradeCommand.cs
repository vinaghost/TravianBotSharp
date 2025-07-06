namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class UpgradeCommand
    {
        public sealed record Command(VillageId VillageId, int Location, bool IsMaxLevel) : IVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AddJobCommand.Handler addJobCommand, GetLayoutBuildingsCommand.Handler getLayoutBuildingsQuery,
            CancellationToken cancellationToken
            )
        {
            var (villageId, location, isMaxLevel) = command;
            var buildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId));
            var building = buildings.Find(x => x.Location == location);

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

            await addJobCommand.HandleAsync(new(villageId, plan.ToJob()));
        }
    }
}