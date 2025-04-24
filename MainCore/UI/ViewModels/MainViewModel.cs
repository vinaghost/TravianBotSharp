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
        private readonly MainWindowLoaded.Handler _mainWindowLoaded;
        private readonly MainWindowUnloaded.Handler _mainWindowUnloaded;

        public MainViewModel(IWaitingOverlayViewModel waitingOverlayViewModel, MainWindowLoaded.Handler mainWindowLoaded, MainWindowUnloaded.Handler mainWindowUnloaded)
        {
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _mainWindowLoaded = mainWindowLoaded;
            _mainWindowUnloaded = mainWindowUnloaded;
        }

        [ReactiveCommand]
        private async Task Load()
        {
            await _waitingOverlayViewModel.Show();
            await _mainWindowLoaded.HandleAsync(new());

            await _waitingOverlayViewModel.ChangeMessage("loading program layout");
            MainLayoutViewModel = Locator.Current.GetService<MainLayoutViewModel>();
            await MainLayoutViewModel.Load();
            await _waitingOverlayViewModel.Hide();
        }

        [ReactiveCommand]
        private async Task Unload()
        {
            await _mainWindowUnloaded.HandleAsync(new());
        }
    }
}