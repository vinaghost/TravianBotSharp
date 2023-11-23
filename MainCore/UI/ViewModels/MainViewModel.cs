using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using MediatR;
using ReactiveUI;
using Splat;

namespace MainCore.UI.ViewModels
{
    [RegisterAsTransient(withoutInterface: true)]
    public class MainViewModel : ViewModelBase
    {
        private MainLayoutViewModel _mainLayoutViewModel;
        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IMediator _mediator;

        public MainViewModel(IMediator mediator, WaitingOverlayViewModel waitingOverlayViewModel)
        {
            _mediator = mediator;
            _waitingOverlayViewModel = waitingOverlayViewModel;
        }

        public async Task Load()
        {
            await _waitingOverlayViewModel.Show();
            await _mediator.Publish(new MainWindowLoaded());

            await _waitingOverlayViewModel.ChangeMessage("loading program layout");
            MainLayoutViewModel = Locator.Current.GetService<MainLayoutViewModel>();
            await MainLayoutViewModel.Load();
            await _waitingOverlayViewModel.Hide();
        }

        public async Task Unload()
        {
            await _mediator.Publish(new MainWindowUnloaded());
        }

        public MainLayoutViewModel MainLayoutViewModel
        {
            get => _mainLayoutViewModel;
            set => this.RaiseAndSetIfChanged(ref _mainLayoutViewModel, value);
        }
    }
}