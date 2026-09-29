using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using ReactiveUI.Primitives.Extensions;
using ReactiveUI.Primitives.Signals;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MainCore.UI.ViewModels
{
    [RegisterSingleton<MainViewModel>]
    public partial class MainViewModel(
        IWaitingOverlayViewModel waitingOverlayViewModel,
        IDbContextFactory<AppDbContext> contextFactory,
        IChromeManager chromeManager)
        : ViewModelBase
    {
        [Reactive]
        private MainLayoutViewModel _mainLayoutViewModel = null!;

        public async Task Load()
        {
            await waitingOverlayViewModel.Show();

            await Signal.Start(() =>
            {
                CheckDriver();

                using var context = contextFactory.CreateDbContext();
                var notExist = context.Database.EnsureCreated();

                if (!notExist)
                {
                    context.FillAccountSettings();
                    context.FillVillageSettings();

                    context.QueueBuildings
                        .Where(x => x.Level == -1)
                        .ExecuteDelete();
                }
            }, RxSchedulers.TaskpoolScheduler);

            await waitingOverlayViewModel.ChangeMessage("loading program layout");
            MainLayoutViewModel = Locator.Current.GetService<MainLayoutViewModel>()!;
            await MainLayoutViewModel.Load();

            await waitingOverlayViewModel.Hide();
        }

        public async Task Unload()
        {
            await chromeManager.Shutdown();
        }

        private static void CheckDriver()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("Your operating system is not supported.");

            string chromePath = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe", null, null) as string ?? throw new Exception("Google Chrome not found in registry");
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(chromePath);
            if (fileVersionInfo.FileVersion is null)
            {
                throw new Exception("Failed to get chrome version");
            }
            var chromeVersion = new Version(fileVersionInfo.FileVersion);
            if (chromeVersion.Major <= 114)
            {
                throw new Exception($"Your chrome version is {chromeVersion}. Please update your chrome first");
            }
        }
    }
}