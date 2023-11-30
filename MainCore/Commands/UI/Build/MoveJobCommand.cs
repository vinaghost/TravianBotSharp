using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.ViewModels.UserControls;
using MediatR;

namespace MainCore.Commands.UI.Build
{
    public class MoveJobCommand : ByAccountVillageIdBase, IRequest
    {
        public ListBoxItemViewModel Jobs { get; }
        public MoveEnums Move { get; }

        public MoveJobCommand(AccountId accountId, VillageId villageId, ListBoxItemViewModel jobs, MoveEnums move) : base(accountId, villageId)
        {
            Jobs = jobs;
            Move = move;
        }
    }

    public class MoveJobCommandHandler : IRequestHandler<MoveJobCommand>
    {
        private readonly IMediator _mediator;
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;
        private readonly IUnitOfRepository _unitOfRepository;

        public MoveJobCommandHandler(IMediator mediator, IDialogService dialogService, ITaskManager taskManager, IUnitOfRepository unitOfRepository)
        {
            _mediator = mediator;
            _dialogService = dialogService;
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(MoveJobCommand request, CancellationToken cancellationToken)
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

            var move = request.Move;
            if (!IsValid(move, oldIndex, jobs.Count)) return;
            var newIndex = GetNewIndex(move, oldIndex, jobs.Count);

            var villageId = request.VillageId;

            var oldJob = jobs[oldIndex];
            var newJob = jobs[newIndex];

            await Task.Run(() => _unitOfRepository.JobRepository.Move(new JobId(oldJob.Id), new JobId(newJob.Id)), cancellationToken);
            await _mediator.Publish(new JobUpdated(accountId, villageId), cancellationToken);
            jobs.SelectedIndex = newIndex;
        }

        public static bool IsValid(MoveEnums move, int oldIndex, int count)
        {
            switch (move)
            {
                case MoveEnums.Top:
                    return oldIndex != 0;

                case MoveEnums.Up:
                    return oldIndex != 0;

                case MoveEnums.Down:
                    return oldIndex != count - 1;

                case MoveEnums.Bottom:
                    return oldIndex != count - 1;

                default:
                    return true; ;
            }
        }

        public static int GetNewIndex(MoveEnums move, int oldIndex, int count)
        {
            switch (move)
            {
                case MoveEnums.Top:
                    return 0;

                case MoveEnums.Up:
                    return oldIndex - 1;

                case MoveEnums.Down:
                    return oldIndex + 1;

                case MoveEnums.Bottom:
                    return count - 1;

                default:
                    return 0;
            }
        }
    }
}