using FluentValidation;
using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Common.Models;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.Models.Input;
using MediatR;

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
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IMediator _mediator;
        private readonly IValidator<ResourceBuildInput> _resourceBuildInputValidator;

        public BuildResourceCommandHandler(ITaskManager taskManager, IDialogService dialogService, IUnitOfRepository unitOfRepository, IMediator mediator, IValidator<ResourceBuildInput> resourceBuildInputValidator)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;
            _resourceBuildInputValidator = resourceBuildInputValidator;
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
            await Task.Run(() => _unitOfRepository.JobRepository.Add(villageId, plan), cancellationToken);
            await _mediator.Publish(new JobUpdated(accountId, villageId), cancellationToken);
        }
    }
}