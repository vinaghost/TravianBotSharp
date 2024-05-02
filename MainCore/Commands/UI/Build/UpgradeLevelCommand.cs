using MainCore.Common.Models;

namespace MainCore.Commands.UI.Build
{
    public class UpgradeLevelCommand : ByAccountVillageIdBase, IRequest
    {
        public int Location { get; }
        public bool IsMaxLevel { get; }

        public UpgradeLevelCommand(AccountId accountId, VillageId villageId, int location, bool isMaxLevel) : base(accountId, villageId)
        {
            Location = location;
            IsMaxLevel = isMaxLevel;
        }
    }

    public class UpgradeLevelCommandHandler : IRequestHandler<UpgradeLevelCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IJobRepository _jobRepository;

        public UpgradeLevelCommandHandler(ITaskManager taskManager, IDialogService dialogService, IMediator mediator, IBuildingRepository buildingRepository, IJobRepository jobRepository)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
            _mediator = mediator;
            _buildingRepository = buildingRepository;
            _jobRepository = jobRepository;
        }

        public async Task Handle(UpgradeLevelCommand request, CancellationToken cancellationToken)
        {
            var status = _taskManager.GetStatus(request.AccountId);
            if (status == StatusEnums.Online)
            {
                _dialogService.ShowMessageBox("Warning", "Please pause account before modifing building queue");
                return;
            }

            var buildings = _buildingRepository.GetBuildings(request.VillageId);
            var building = buildings.FirstOrDefault(x => x.Location == request.Location);

            if (building is null) return;
            if (building.Type == BuildingEnums.Site) return;

            var level = 0;

            if (request.IsMaxLevel)
            {
                level = building.Type.GetMaxLevel();
            }
            else
            {
                level = building.Level + 1;
            }

            var plan = new NormalBuildPlan()
            {
                Location = request.Location,
                Type = building.Type,
                Level = level,
            };

            _jobRepository.Add(request.VillageId, plan);
            await _mediator.Publish(new JobUpdated(request.AccountId, request.VillageId), cancellationToken);
        }
    }
}