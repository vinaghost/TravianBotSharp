using Avalonia.Controls;

using MainCore.UI.ViewModels;
using MainCore.UI.ViewModels.UserControls;

using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace AvaloniaUI.Views
{
    public class MainWindowBase : ReactiveWindow<MainViewModel>
    {
    }

    public partial class MainWindow : MainWindowBase
    {
        private bool _canClose = false;
        private bool _isClosing = false;
        private bool _isLoaded = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainViewModel mainViewModel, WaitingOverlayViewModel waitingOverlayViewModel) : this()
        {
            Opened += OnOpened;
            Closing += OnClosing;

            ViewModel = mainViewModel;
            WaitingOverlay.ViewModel = waitingOverlayViewModel;

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.MainLayoutViewModel, v => v.MainLayout.Content).DisposeWith(d);
            });
        }

        private async void OnOpened(object? sender, EventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = false;
            await ViewModel!.Load();
            _isLoaded = true;
        }

        private async void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            if (!_isLoaded)
            {
                e.Cancel = true;
                return;
            }

            if (_canClose) return;
            e.Cancel = true;
            if (_isClosing) return;
            _isClosing = true;

            await ViewModel!.Unload();

            _canClose = true;

            Signal.After(TimeSpan.FromMilliseconds(100), RxSchedulers.MainThreadScheduler).Subscribe(_ => Close());
        }
    }
}
