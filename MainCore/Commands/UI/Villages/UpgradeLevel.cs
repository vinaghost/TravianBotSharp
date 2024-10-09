using MainCore.Common.Models;

namespace MainCore.Commands.UI.Tabs.Villages
{
    [RegisterSingleton<UpgradeLevel>]
    public class UpgradeLevel
    {
        private readonly IMediator _mediator;
        private readonly GetBuildings _getBuildings;

        public UpgradeLevel(IMediator mediator, GetBuildings getBuildings)
        {
            _mediator = mediator;
            _getBuildings = getBuildings;
        }

        public async Task Execute(AccountId accountId, VillageId villageId, int location, bool isMaxLevel)
        {
            var buildings = _getBuildings.Layout(villageId);
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

            var addJobCommand = Locator.Current.GetService<AddJobCommand>();
            addJobCommand.ToBottom(villageId, plan);
            await _mediator.Publish(new JobUpdated(accountId, villageId));
        }
    }
}