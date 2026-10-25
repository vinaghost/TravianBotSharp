using System.Diagnostics;
using System.Runtime.CompilerServices;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

using AvaloniaUI.Views;

using MainCore;
using MainCore.Services;
using MainCore.UI.ViewModels;
using MainCore.UI.ViewModels.UserControls;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ReactiveUI.Avalonia;
using ReactiveUI.Builder;
using ReactiveUI.Primitives;

using Serilog;

using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace AvaloniaUI
{
    public partial class App : Application
    {
        private static readonly FilePickerFileType TbsFileType = new("TBS files")
        {
            Patterns = ["*.tbs"],
        };

        private IHost? _host;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // WPFUI lets a hosting package run the UI on its own thread. macOS only allows
                // windows on the process main thread, so the host is built here instead and
                // ReactiveUI is registered once, straight into the Microsoft DI container.
                _host = AppMixins.GetHostBuilder()
                    .ConfigureServices(services =>
                    {
                        services.UseMicrosoftDependencyResolver();
                        AppLocator.CurrentMutable.CreateReactiveUIBuilder()
                            .WithExceptionHandler(new LoggingExceptionObserver())
                            .WithAvalonia()
                            .WithCoreServices()
                            .BuildApp();
                    })
                    .Build();

                _host.Services.UseMicrosoftDependencyResolver();
                SetupDialogService(_host.Services);
                _host.Services.GetRequiredService<RxQueue>().Setup();

                _host.Start();

                desktop.MainWindow = new MainWindow(
                    _host.Services.GetRequiredService<MainViewModel>(),
                    _host.Services.GetRequiredService<WaitingOverlayViewModel>());

                // The host swallows SIGTERM / Ctrl+C. Close the window instead so the browsers
                // get shut down the same way as when the user closes it.
                _host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.Register(() =>
                    Dispatcher.UIThread.Post(() => desktop.MainWindow?.Close()));

                desktop.Exit += OnExit;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            if (_host is not null)
            {
                _host.StopAsync().GetAwaiter().GetResult();
                _host.Dispose();
            }
            Log.CloseAndFlush();
        }

        private static void SetupDialogService(IServiceProvider serviceProvider)
        {
            var dialogService = serviceProvider.GetRequiredService<DialogService>();
            dialogService.MessageBox.RegisterHandler(async context =>
            {
                await Dispatcher.UIThread.InvokeAsync(() => MessageDialog.Show(context.Input.Title, context.Input.Message, isConfirm: false));
                context.SetOutput(RxVoid.Default);
            });

            dialogService.ConfirmBox.RegisterHandler(async context =>
            {
                var result = await Dispatcher.UIThread.InvokeAsync(() => MessageDialog.Show(context.Input.Title, context.Input.Message, isConfirm: true));
                context.SetOutput(result);
            });

            dialogService.FileOpenDialog.RegisterHandler(async context =>
            {
                var result = await Dispatcher.UIThread.InvokeAsync(OpenFileDialog);
                context.SetOutput(result);
            });

            dialogService.FileSaveDialog.RegisterHandler(async context =>
            {
                var result = await Dispatcher.UIThread.InvokeAsync(SaveFileDialog);
                context.SetOutput(result);
            });
        }

        internal static Window? GetMainWindow()
        {
            return (Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        }

        private static async Task<string> SaveFileDialog()
        {
            var window = GetMainWindow();
            if (window is null) return "";

            var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                SuggestedStartLocation = await window.StorageProvider.TryGetFolderFromPathAsync(AppPaths.Root),
                DefaultExtension = "tbs",
                FileTypeChoices = [TbsFileType, FilePickerFileTypes.All],
                ShowOverwritePrompt = true,
            });
            return file?.TryGetLocalPath() ?? "";
        }

        private static async Task<string> OpenFileDialog()
        {
            var window = GetMainWindow();
            if (window is null) return "";

            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                SuggestedStartLocation = await window.StorageProvider.TryGetFolderFromPathAsync(AppPaths.Root),
                AllowMultiple = false,
                FileTypeFilter = [TbsFileType, FilePickerFileTypes.All],
            });
            if (files.Count == 0) return "";
            return files[0].TryGetLocalPath() ?? "";
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
                Dispatcher.UIThread.Post(() => _ = MessageDialog.Show("Error", "There is something wrong. Please check logs/logs-Other.txt.", isConfirm: false));

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
