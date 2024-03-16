using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
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

        public TrainTroopCommandHandler(IDialogService dialogService, ITaskManager taskManager)
        {
            _dialogService = dialogService;
            _taskManager = taskManager;
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
            _dialogService.ShowMessageBox("Work in progress", $"{type} {amount} {great}");
        }
    }
}