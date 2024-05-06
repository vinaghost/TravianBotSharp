﻿using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Commands.UI.Build
{
    public class DeleteAllJobCommand : ByAccountVillageIdBase, IRequest
    {
        public ListBoxItemViewModel Jobs { get; }

        public DeleteAllJobCommand(AccountId accountId, VillageId villageId, ListBoxItemViewModel jobs) : base(accountId, villageId)
        {
            Jobs = jobs;
        }
    }

    public class DeleteAllJobCommandHandler : IRequestHandler<DeleteAllJobCommand>
    {
        private readonly IMediator _mediator;
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;
        private readonly IJobRepository _jobRepository;

        public DeleteAllJobCommandHandler(IMediator mediator, IDialogService dialogService, ITaskManager taskManager, IJobRepository jobRepository)
        {
            _mediator = mediator;
            _dialogService = dialogService;
            _taskManager = taskManager;
            _jobRepository = jobRepository;
        }

        public async Task Handle(DeleteAllJobCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var status = _taskManager.GetStatus(accountId);
            if (status == StatusEnums.Online)
            {
                _dialogService.ShowMessageBox("Warning", "Please pause account before modifing building queue");
                return;
            }
            var villageId = request.VillageId;
            await Task.Run(() => _jobRepository.Delete(villageId), cancellationToken);
            await _mediator.Publish(new JobUpdated(accountId, villageId), cancellationToken);
        }
    }
}