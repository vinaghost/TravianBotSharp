using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Commands.Abstract
{
    public abstract class CommandUIBase(IDialogService dialogService, IWaitingOverlayViewModel waitingOverlayViewModel, IMediator mediator)
    {
        protected readonly IDialogService _dialogService = dialogService;
        protected readonly IWaitingOverlayViewModel _waitingOverlayViewModel = waitingOverlayViewModel;
        protected readonly IMediator _mediator = mediator;
    }
}