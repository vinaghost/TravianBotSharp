using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace MainCore.UI.ViewModels
{
    [RegisterSingleton<MainViewModel>]
    public partial class MainViewModel : ViewModelBase
    {
        [Reactive]
        private MainLayoutViewModel _mainLayoutViewModel;

        private readonly IWaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IMediator _mediator;

        public MainViewModel(IMediator mediator, IWaitingOverlayViewModel waitingOverlayViewModel)
        {
            _mediator = mediator;
            _waitingOverlayViewModel = waitingOverlayViewModel;
        }

        [ReactiveCommand]
        private async Task Load()
        {
            await _waitingOverlayViewModel.Show();
            await _mediator.Publish(new MainWindowLoaded());

            await _waitingOverlayViewModel.ChangeMessage("loading program layout");
            MainLayoutViewModel = Locator.Current.GetService<MainLayoutViewModel>();
            await MainLayoutViewModel.Load();
            await _waitingOverlayViewModel.Hide();
        }

        [ReactiveCommand]
        private async Task Unload()
        {
            await _mediator.Publish(new MainWindowUnloaded());
        }
    }
}