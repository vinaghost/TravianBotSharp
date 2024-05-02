using FluentValidation;
using MainCore.Common.Models;
using MainCore.UI.Models.Input;

namespace MainCore.Commands.UI.Build
{
    public class BuildResourceCommand : ByAccountVillageIdBase, IRequest
    {
        public ResourceBuildInput ResourceBuildInput { get; }

        public BuildResourceCommand(AccountId accountId, VillageId villageId, ResourceBuildInput resourceBuildInput) : base(accountId, villageId)
        {
            ResourceBuildInput = resourceBuildInput;
        }
    }

    public class BuildResourceCommandHandler : IRequestHandler<BuildResourceCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly IValidator<ResourceBuildInput> _resourceBuildInputValidator;
        private readonly IJobRepository _jobRepository;

        public BuildResourceCommandHandler(ITaskManager taskManager, IDialogService dialogService, IMediator mediator, IValidator<ResourceBuildInput> resourceBuildInputValidator, IJobRepository jobRepository)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
            _mediator = mediator;
            _resourceBuildInputValidator = resourceBuildInputValidator;
            _jobRepository = jobRepository;
        }

        public async Task Handle(BuildResourceCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villageId = request.VillageId;
            var ResourceBuildInput = request.ResourceBuildInput;

            var status = _taskManager.GetStatus(accountId);
            if (status == StatusEnums.Online)
            {
                _dialogService.ShowMessageBox("Warning", "Please pause account before modifing building queue");
                return;
            }
            var result = _resourceBuildInputValidator.Validate(ResourceBuildInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            var (type, level) = ResourceBuildInput.Get();
            var plan = new ResourceBuildPlan()
            {
                Plan = type,
                Level = level,
            };
            await Task.Run(() => _jobRepository.Add(villageId, plan), cancellationToken);
            await _mediator.Publish(new JobUpdated(accountId, villageId), cancellationToken);
        }
    }
}