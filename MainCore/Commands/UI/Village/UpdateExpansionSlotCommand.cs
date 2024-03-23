using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MainCore.Tasks;
using MainCore.UI.ViewModels.UserControls;
using MediatR;

namespace MainCore.Commands.UI.Village
{
    public class UpdateExpansionSlotCommand : ByAccountIdBase, IRequest
    {
        public UpdateExpansionSlotCommand(AccountId accountId, ListBoxItemViewModel villages) : base(accountId)
        {
            Villages = villages;
        }

        public ListBoxItemViewModel Villages { get; }
    }

    public class UpdateExpansionSlotCommandHandler : IRequestHandler<UpdateExpansionSlotCommand>
    {
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;

        public UpdateExpansionSlotCommandHandler(ITaskManager taskManager, IDialogService dialogService)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
        }

        public async Task Handle(UpdateExpansionSlotCommand request, CancellationToken cancellationToken)
        {
            var villages = request.Villages;
            if (!villages.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No village selected");
                return;
            }
            var accountId = request.AccountId;
            var villageId = new VillageId(villages.SelectedItemId);

            await _taskManager.AddOrUpdate<UpdateExpansionSlotTask>(accountId, villageId);

            _dialogService.ShowMessageBox("Information", $"Added update expansion task");
        }
    }
}