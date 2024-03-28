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
    public class TrainTroopCommand : ByAccountVillageIdBase, IRequest
    {
        public TrainTroopInput TrainTroopInput { get; }

        public TrainTroopCommand(AccountId accountId, VillageId villageId, TrainTroopInput trainTroopInput) : base(accountId, villageId)
        {
            TrainTroopInput = trainTroopInput;
        }
    }

    public class TrainTroopCommandHandler : IRequestHandler<TrainTroopCommand>
    {
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IMediator _mediator;

        public TrainTroopCommandHandler(IDialogService dialogService, ITaskManager taskManager, UnitOfRepository unitOfRepository, IMediator mediator)
        {
            _dialogService = dialogService;
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;
        }

        public async Task Handle(TrainTroopCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = request.AccountId;

            var status = _taskManager.GetStatus(accountId);
            if (status == StatusEnums.Online)
            {
                _dialogService.ShowMessageBox("Warning", "Please pause account before modifing building queue");
                return;
            }

            var input = request.TrainTroopInput;
            var (type, amount, great) = input.Get();
            var plan = new TrainTroopPlan()
            {
                Type = type,
                Amount = amount,
                Great = great
            };
            var villageId = request.VillageId;

            await Task.Run(() => _unitOfRepository.JobRepository.Add(villageId, plan));
            await _mediator.Publish(new JobUpdated(accountId, villageId), cancellationToken);
        }
    }
}