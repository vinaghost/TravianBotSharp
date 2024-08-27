using FluentValidation;
using Humanizer;
using MainCore.Common.Models;
using MainCore.UI.Models.Input;

namespace MainCore.Commands.UI.Build
{
    public class BuildNormalCommand
    {
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;

        private readonly ITaskManager _taskManager;
        private readonly IValidator<NormalBuildInput> _normalBuildInputValidator;

        public BuildNormalCommand(IDialogService dialogService = null, IMediator mediator = null, ITaskManager taskManager = null, IValidator<NormalBuildInput> normalBuildInputValidator = null)
        {
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();
            _mediator = mediator ?? Locator.Current.GetService<IMediator>();
            _taskManager = taskManager ?? Locator.Current.GetService<ITaskManager>();
            _normalBuildInputValidator = normalBuildInputValidator ?? Locator.Current.GetService<IValidator<NormalBuildInput>>();
        }

        public async Task Execute(AccountId accountId, VillageId villageId, NormalBuildInput normalBuildInput, int location)
        {
            var status = _taskManager.GetStatus(accountId);
            if (status == StatusEnums.Online)
            {
                _dialogService.ShowMessageBox("Warning", "Please pause account before modifing building queue");
                return;
            }

            var result = await _normalBuildInputValidator.ValidateAsync(normalBuildInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            var (type, level) = normalBuildInput.Get();
            var plan = new NormalBuildPlan()
            {
                Location = location,
                Type = type,
                Level = level,
            };
            await NormalBuild(accountId, villageId, plan);
        }

        private async Task NormalBuild(AccountId accountId, VillageId villageId, NormalBuildPlan plan)
        {
            var buildings = new GetBuildings().Execute(villageId);

            var building = buildings.Find(x => x.Location == plan.Location);

            if (building is null)
            {
                var result = CheckRequirements(buildings, plan);
                if (result.IsFailed)
                {
                    _dialogService.ShowMessageBox("Error", result.Errors[0].Message);
                    return;
                }
                Validate(buildings, plan);
            }

            new AddJobCommand().ToBottom(villageId, plan);
            await _mediator.Publish(new JobUpdated(accountId, villageId));
        }

        private static Result CheckRequirements(List<BuildingItem> buildings, NormalBuildPlan plan)
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

        private static void Validate(List<BuildingItem> buildings, NormalBuildPlan plan)
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
    }
}