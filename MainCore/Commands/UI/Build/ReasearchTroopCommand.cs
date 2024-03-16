using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MainCore.UI.Models.Input;
using MediatR;

namespace MainCore.Commands.UI.Build
{
    public class ResearchTroopCommand : ByAccountVillageIdBase, IRequest
    {
        public ResearchTroopInput ResearchTroopInput { get; }

        public ResearchTroopCommand(AccountId accountId, VillageId villageId, ResearchTroopInput researchTroopInput) : base(accountId, villageId)
        {
            ResearchTroopInput = researchTroopInput;
        }
    }

    public class ResearchTroopCommandHandler : IRequestHandler<ResearchTroopCommand>
    {
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;

        public ResearchTroopCommandHandler(IDialogService dialogService, ITaskManager taskManager)
        {
            _dialogService = dialogService;
            _taskManager = taskManager;
        }

        public async Task Handle(ResearchTroopCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = request.AccountId;

            var status = _taskManager.GetStatus(accountId);
            if (status == StatusEnums.Online)
            {
                _dialogService.ShowMessageBox("Warning", "Please pause account before modifing building queue");
                return;
            }

            var input = request.ResearchTroopInput;
            var type = input.Get();
            _dialogService.ShowMessageBox("Work in progress", $"{type} ");
        }
    }
}