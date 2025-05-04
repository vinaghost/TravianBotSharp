using MainCore.Commands.Base;
using MainCore.Common.Models;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class UpgradeCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, int Location, bool IsMaxLevel) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            AddJobCommand.Handler addJobCommand, GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery,
            CancellationToken cancellationToken
            )
        {
            var (accountId, villageId, location, isMaxLevel) = command;
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

            await addJobCommand.HandleAsync(new(accountId, villageId, plan.ToJob(villageId)));
        }
    }
}