using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MainCore.UI.Models.Input;
using MediatR;

namespace MainCore.Commands.UI.Build
{
    public class CelebrationCommand : ByAccountVillageIdBase, IRequest
    {
        public CelebrationInput CelebrationInput { get; }

        public CelebrationCommand(AccountId accountId, VillageId villageId, CelebrationInput celebrationInput) : base(accountId, villageId)
        {
            CelebrationInput = celebrationInput;
        }
    }

    public class CelebrationCommandHandler : IRequestHandler<CelebrationCommand>
    {
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;

        public CelebrationCommandHandler(IDialogService dialogService, ITaskManager taskManager)
        {
            _dialogService = dialogService;
            _taskManager = taskManager;
        }

        public async Task Handle(CelebrationCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = request.AccountId;

            var status = _taskManager.GetStatus(accountId);
            if (status == StatusEnums.Online)
            {
                _dialogService.ShowMessageBox("Warning", "Please pause account before modifing building queue");
                return;
            }

            var input = request.CelebrationInput;
            var type = input.Get();
            _dialogService.ShowMessageBox("Work in progress", $"{type} ");
        }
    }
}