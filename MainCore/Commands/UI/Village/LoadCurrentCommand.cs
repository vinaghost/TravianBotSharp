using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MainCore.Tasks;
using MainCore.UI.ViewModels.UserControls;
using MediatR;

namespace MainCore.Commands.UI.Village
{
    public class LoadCurrentCommand : ByAccountIdBase, IRequest
    {
        public ListBoxItemViewModel Villages { get; }

        public LoadCurrentCommand(AccountId accountId, ListBoxItemViewModel villages) : base(accountId)
        {
            Villages = villages;
        }
    }

    public class LoadCurrentCommandHandler : IRequestHandler<LoadCurrentCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;

        public LoadCurrentCommandHandler(ITaskManager taskManager, IDialogService dialogService)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
        }

        public async Task Handle(LoadCurrentCommand request, CancellationToken cancellationToken)
        {
            var villages = request.Villages;
            if (!villages.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No village selected");
                return;
            }
            var accountId = request.AccountId;
            var villageId = new VillageId(villages.SelectedItemId);
            await _taskManager.AddOrUpdate<UpdateBuildingTask>(accountId, villageId);

            _dialogService.ShowMessageBox("Information", $"Added update task");
            return;
        }
    }
}