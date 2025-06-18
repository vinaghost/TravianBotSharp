using MainCore;
using MainCore.Services;
using MainCore.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;
using ReactiveMarbles.Extensions.Hosting.Wpf;
using ReactiveUI;
using Splat.ModeDetection;
using System;
using System.IO;
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
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = (Exception)args.ExceptionObject;
                File.WriteAllText("crash.log", ex.ToString());
            };

            Splat.ModeDetector.OverrideModeDetector(Mode.Run);
            var host = AppMixins.GetHostBuilder()
                .ConfigureWpf(wpfBuilder => wpfBuilder.UseCurrentApplication(this).UseWindow<MainWindow>())
                .UseWpfLifetime()
                .Build();

            host.MapSplatLocator(sp =>
            {
                RxApp.DefaultExceptionHandler = sp.GetRequiredService<ObservableExceptionHandler>();
                SetupDialogService(sp);
            });

            host.RunAsync();
        }

        private static void SetupDialogService(IServiceProvider serviceProvider)
        {
            var dialogService = serviceProvider.GetRequiredService<IDialogService>();
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
    }
}