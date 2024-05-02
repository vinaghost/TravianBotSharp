using FluentValidation;
using Humanizer;
using MainCore.Common.Extensions;
using MainCore.Common.Models;
using MainCore.UI.Models.Input;

namespace MainCore.Commands.UI.Build
{
    public class BuildNormalCommand : ByAccountVillageIdBase, IRequest
    {
        public NormalBuildInput NormalBuildInput { get; }

        public int Location { get; }

        public BuildNormalCommand(AccountId accountId, VillageId villageId, NormalBuildInput normalBuildInput, int location) : base(accountId, villageId)
        {
            NormalBuildInput = normalBuildInput;
            Location = location;
        }
    }

    public class BuildNormalCommandHandler : IRequestHandler<BuildNormalCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IValidator<NormalBuildInput> _normalBuildInputValidator;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IJobRepository _jobRepository;

        public BuildNormalCommandHandler(ITaskManager taskManager, IValidator<NormalBuildInput> normalBuildInputValidator, IDialogService dialogService, IMediator mediator, IBuildingRepository buildingRepository, IJobRepository jobRepository)
        {
            _taskManager = taskManager;
            _normalBuildInputValidator = normalBuildInputValidator;
            _dialogService = dialogService;
            _mediator = mediator;
            _buildingRepository = buildingRepository;
            _jobRepository = jobRepository;
        }

        public async Task Handle(BuildNormalCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villageId = request.VillageId;
            var normalBuildInput = request.NormalBuildInput;
            var status = _taskManager.GetStatus(accountId);
            if (status == StatusEnums.Online)
            {
                _dialogService.ShowMessageBox("Warning", "Please pause account before modifing building queue");
                return;
            }

            var result = _normalBuildInputValidator.Validate(normalBuildInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            var (type, level) = normalBuildInput.Get();
            var plan = new NormalBuildPlan()
            {
                Location = request.Location,
                Type = type,
                Level = level,
            };
            await NormalBuild(accountId, villageId, plan, cancellationToken);
        }

        private async Task NormalBuild(AccountId accountId, VillageId villageId, NormalBuildPlan plan, CancellationToken cancellationToken)
        {
            var buildings = _buildingRepository.GetBuildings(villageId);
            var result = CheckRequirements(buildings, plan);
            if (result.IsFailed)
            {
                _dialogService.ShowMessageBox("Error", result.Errors.First().Message);
                return;
            }

            Validate(buildings, plan);

            await Task.Run(() => _jobRepository.Add(villageId, plan));
            await _mediator.Publish(new JobUpdated(accountId, villageId), cancellationToken);
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
                if (sameTypeBuildings.Where(x => x.Location == plan.Location).Any()) return;
                var largestLevelBuilding = sameTypeBuildings.MaxBy(x => x.Level);
                if (largestLevelBuilding.Level == plan.Type.GetMaxLevel()) return;
                plan.Location = largestLevelBuilding.Location;
                return;
            }

            if (plan.Type.IsResourceField())
            {
                var field = buildings.FirstOrDefault(x => x.Location == plan.Location);
                if (plan.Type == field.Type) return;
                plan.Type = field.Type;
                return;
            }

            {
                var building = buildings.FirstOrDefault(x => x.Type == plan.Type);
                if (building is null) return;
                if (plan.Location == building.Location) return;
                plan.Location = building.Location;
            }
        }
    }
}