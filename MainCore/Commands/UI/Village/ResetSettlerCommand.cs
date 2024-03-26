using MainCore.Entities;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.ViewModels.UserControls;
using MediatR;

namespace MainCore.Commands.UI.Village
{
    public class ResetSettlerCommand : IRequest
    {
        public ListBoxItemViewModel Villages { get; }

        public ResetSettlerCommand(ListBoxItemViewModel villages)
        {
            Villages = villages;
        }
    }

    public class ResetSettlerCommandHandler : IRequestHandler<ResetSettlerCommand>
    {
        private readonly IDialogService _dialogService;
        private readonly UnitOfRepository _unitOfRepository;

        public ResetSettlerCommandHandler(IDialogService dialogService, UnitOfRepository unitOfRepository)
        {
            _dialogService = dialogService;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(ResetSettlerCommand request, CancellationToken cancellationToken)
        {
            var villages = request.Villages;
            if (!villages.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No village selected");
                return;
            }
            var villageId = new VillageId(villages.SelectedItemId);
            _unitOfRepository.VillageRepository.SetSettlers(villageId, 0);
            _dialogService.ShowMessageBox("Information", $"Complete set settler to 0");
            await Task.CompletedTask;
        }
    }
}