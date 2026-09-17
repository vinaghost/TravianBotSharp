using MainCore.UI.ViewModels;
using MainCore.UI.ViewModels.UserControls;
using ReactiveMarbles.Extensions.Hosting.Wpf;
using System;
using System.ComponentModel;
using System.Windows;

namespace WPFUI.Views
{
    using ReactiveUI;
    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Extensions;

    public class MainWindowBase : ReactiveWindow<MainViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MainWindowBase, IWpfShell
    {
        private bool _canClose = false;
        private bool _isClosing = false;
        private bool _isLoaded = false;

        public MainWindow(MainViewModel mainViewModel, WaitingOverlayViewModel waitingOverlayViewModel)
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Closing += OnClosing;

            ViewModel = mainViewModel;
            WaitingOverlay.ViewModel = waitingOverlayViewModel;

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.MainLayoutViewModel, v => v.MainLayout.Content).DisposeWith(d);
            });
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = false;
            await ViewModel.LoadCommand.Execute().ToHotTask();
            _isLoaded = true;
        }

        private async void OnClosing(object sender, CancelEventArgs e)
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

            await ViewModel.UnloadCommand.Execute().ToHotTask();

            _canClose = true;
            Close();
        }
    }
}