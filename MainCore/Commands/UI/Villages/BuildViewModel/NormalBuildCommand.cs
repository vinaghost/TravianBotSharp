using Humanizer;
using MainCore.Commands.Base;
using MainCore.Common.Models;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class NormalBuildCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, NormalBuildPlan plan) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            IDialogService dialogService,
            GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery, AddJobCommand.Handler addJobCommand,
            CancellationToken cancellationToken
            )
        {
            var (accountId, villageId, plan) = command;

            var buildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId));
            var building = buildings.Find(x => x.Location == plan.Location);

            if (building is null)
            {
                var result = plan.CheckRequirements(buildings);
                if (result.IsFailed)
                {
                    await dialogService.MessageBox.Handle(new MessageBoxData("Error", result.Errors[0].Message));
                    return;
                }
                plan.ValidateLocation(buildings);
            }

            await addJobCommand.HandleAsync(new(accountId, villageId, plan.ToJob(villageId)));
        }

        private static Result CheckRequirements(this NormalBuildPlan plan, List<BuildingItem> buildings)
        {
            var prerequisiteBuildings = plan.Type.GetPrerequisiteBuildings();
            if (prerequisiteBuildings.Count == 0) return Result.Ok();
            foreach (var prerequisiteBuilding in prerequisiteBuildings)
            {
                var valid = buildings
                    .Where(x => x.Type == prerequisiteBuilding.Type)
                    .Any(x => x.Level >= prerequisiteBuilding.Level);

                if (!valid) return Result.Fail($"Required {prerequisiteBuilding.Type.Humanize()} lvl {prerequisiteBuilding.Level}");
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
                var largestLevelBuilding = sameTypeBuildings.MaxBy(x => x.Level);
                if (largestLevelBuilding.Level == plan.Type.GetMaxLevel()) return;
                plan.Location = largestLevelBuilding.Location;
                return;
            }

            if (plan.Type.IsResourceField())
            {
                var field = buildings.Find(x => x.Location == plan.Location);
                if (plan.Type == field.Type) return;
                plan.Type = field.Type;
                return;
            }

            var building = buildings.Find(x => x.Type == plan.Type);
            if (building is null) return;
            if (plan.Location == building.Location) return;
            plan.Location = building.Location;
        }

        public static NormalBuildPlan ToPlan(this NormalBuildInput input, int location)
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