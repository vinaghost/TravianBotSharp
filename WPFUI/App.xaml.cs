using MainCore;
using MainCore.Services;
using MainCore.UI;
using MainCore.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Splat;
using Splat.ModeDetection;
using System;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows;
using WPFUI.Views;

namespace WPFUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly MainWindow _mainWindow;

        public IServiceProvider Container { get; private set; }

        public App()
        {
            Splat.ModeDetector.OverrideModeDetector(Mode.Run);
            Container = DependencyInjection.Setup();
            RxApp.DefaultExceptionHandler = Locator.Current.GetService<ObservableExceptionHandler>();

            _mainWindow = new MainWindow()
            {
                ViewModel = Locator.Current.GetService<MainViewModel>(),
            };

            var dialogService = Locator.Current.GetService<IDialogService>();
            dialogService.MessageBox.RegisterHandler(async context =>
            {
                ShowMessage(context.Input.Title, context.Input.Message);
                context.SetOutput(Unit.Default);
                await Task.CompletedTask;
            });

            dialogService.ConfirmBox.RegisterHandler(async context =>
            {
                var result = ShowConfirm(context.Input.Title, context.Input.Message);
                context.SetOutput(result);
                await Task.CompletedTask;
            });

            dialogService.OpenFileDialog.RegisterHandler(async context =>
            {
                var result = OpenFileDialog();
                context.SetOutput(result);
                await Task.CompletedTask;
            });

            dialogService.SaveFileDialog.RegisterHandler(async context =>
            {
                var result = SaveFileDialog();
                context.SetOutput(result);
                await Task.CompletedTask;
            });
        }

        private static void ShowMessage(string title, string message)
        {
            MessageBox.Show(message, title);
        }

        private static bool ShowConfirm(string title, string message)
        {
            var answer = MessageBox.Show(message, title, MessageBoxButton.YesNo);
            return answer == MessageBoxResult.Yes;
        }

        private static string SaveFileDialog()
        {
            var svd = new Microsoft.Win32.SaveFileDialog
            {
                InitialDirectory = AppContext.BaseDirectory,
                Filter = "TBS files (*.tbs)|*.tbs|All files (*.*)|*.*",
            };
            if (svd.ShowDialog() != true) return "";
            return svd.FileName;
        }

        private static string OpenFileDialog()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = AppContext.BaseDirectory,
                Filter = "TBS files (*.tbs)|*.tbs|All files (*.*)|*.*",
            };
            if (ofd.ShowDialog() != true) return "";
            return ofd.FileName;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _mainWindow.Show();
            base.OnStartup(e);
        }
    }
}