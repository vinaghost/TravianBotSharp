using MainCore.Common.MediatR;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Commands.UI.Build
{
    public class DeleteJobCommand : ByAccountVillageIdBase, IRequest
    {
        public ListBoxItemViewModel Jobs { get; }

        public DeleteJobCommand(AccountId accountId, VillageId villageId, ListBoxItemViewModel jobs) : base(accountId, villageId)
        {
            Jobs = jobs;
        }
    }

    public class DeleteJobCommandHandler : IRequestHandler<DeleteJobCommand>
    {
        private readonly IMediator _mediator;
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public DeleteJobCommandHandler(IMediator mediator, IDialogService dialogService, ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _mediator = mediator;
            _dialogService = dialogService;
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(DeleteJobCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var status = _taskManager.GetStatus(accountId);
            if (status == StatusEnums.Online)
            {
                _dialogService.ShowMessageBox("Warning", "Please pause account before modifing building queue");
                return;
            }
            var jobs = request.Jobs;
            if (!jobs.IsSelected) return;
            var oldIndex = jobs.SelectedIndex;
            var jobId = jobs.SelectedItemId;

            await Task.Run(() => _unitOfRepository.JobRepository.Delete(new JobId(jobId)), cancellationToken);
            var villageId = request.VillageId;
            await _mediator.Publish(new JobUpdated(accountId, villageId), cancellationToken);
        }
    }
}