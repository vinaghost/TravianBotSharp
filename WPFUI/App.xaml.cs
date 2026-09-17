using MainCore;
using MainCore.Services;
using MainCore.UI;
using MainCore.UI.Models.Output;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;
using ReactiveMarbles.Extensions.Hosting.Wpf;
using ReactiveUI;
using ReactiveUI.Builder;
using ReactiveUI.Primitives;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using Splat.ModeDetection;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using WPFUI.Views;
using MainCore.UI.ViewModels;
using MainCore.UI.ViewModels.UserControls;

namespace WPFUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>Raises the <see cref="Application.Startup"/> event.</summary>
        /// <param name="e">A <see cref="StartupEventArgs"/> that contains the event data.</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var hostBuilder = AppMixins.GetHostBuilder()
                .ConfigureSplatForMicrosoftDependencyResolver()
                .ConfigureWpf(wpfBuilder => wpfBuilder
                    .UseCurrentApplication(this)
                    .UseWindow(typeof(MainWindow)));

            var host = hostBuilder
                .Build();

            host.MapSplatLocator(sp =>
            {
                SetupDialogService(sp);
                sp.GetRequiredService<IRxQueue>().Setup();
            });

            await host.RunAsync();
        }

        private static void SetupDialogService(IServiceProvider serviceProvider)
        {
            var dialogService = serviceProvider.GetRequiredService<IDialogService>();
            dialogService.MessageBox.RegisterHandler(context =>
            {
                ShowMessage(context.Input.Title, context.Input.Message);
                context.SetOutput(RxVoid.Default);
            });

            dialogService.ConfirmBox.RegisterHandler(context =>
            {
                var result = ShowConfirm(context.Input.Title, context.Input.Message);
                context.SetOutput(result);
            });

            dialogService.FileOpenDialog.RegisterHandler(context =>
            {
                var result = OpenFileDialog();
                context.SetOutput(result);
            });

            dialogService.FileSaveDialog.RegisterHandler(context =>
            {
                var result = SaveFileDialog();
                context.SetOutput(result);
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

        private sealed class LoggingExceptionObserver : IObserver<Exception>
        {
            /// <inheritdoc/>
            public void OnNext(Exception value)
            {
                Trace
                    .TraceError($"[ReactiveUI] Unhandled exception: {value}");
                Log
                    .ForContext<LoggingExceptionObserver>()
                    .Error(value, "Unhandled exception");
                MessageBox.Show("Error", "There is something wrong. Please check logs/logs-Other.txt.");

                if (!Debugger.IsAttached) return;
                Debugger.Break();
            }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnError(Exception error) => OnNext(error);

            /// <inheritdoc/>
            public void OnCompleted()
            {
            }
        }
    }
}