using System.Diagnostics;
using System.Runtime.InteropServices;

using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;

using Microsoft.Win32;

using ReactiveUI.Primitives.Signals;

using Splat;

namespace MainCore.UI.ViewModels
{
    [RegisterSingleton<MainViewModel>]
    public partial class MainViewModel(
        IWaitingOverlayViewModel waitingOverlayViewModel,
        IDbContextFactory<AppDbContext> contextFactory,
        ChromeManager chromeManager)
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
            LogService.Shutdown();
        }

        private static void CheckDriver()
        {
            Version chromeVersion;

            var chromeVersionString = GetChromePath("HKEY_CURRENT_USER\\SOFTWARE\\Google\\Chrome\\BLBeacon", "version");
            if (!string.IsNullOrEmpty(chromeVersionString))
            {
                chromeVersion = new Version(chromeVersionString);
            }
            else
            {
                chromeVersionString = GetChromePath("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Google Chrome", "version");
                if (!string.IsNullOrEmpty(chromeVersionString))
                {
                    chromeVersion = new Version(chromeVersionString);
                }
                else
                {
                    var chromePath = GetChromePath("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe");
                    if (!string.IsNullOrEmpty(chromePath))
                    {
                        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(chromePath);
                        if (!string.IsNullOrEmpty(fileVersionInfo.FileVersion))
                        {
                            chromeVersion = new Version(fileVersionInfo.FileVersion);
                        }
                        else
                        {
                            throw new Exception($"Chrome is installed but failed to get chrome version [{fileVersionInfo.FileVersion}]");
                        }
                    }
                    else
                    {
                        throw new Exception("Chrome is not installed");
                    }
                }
            }

            if (chromeVersion.Major <= 114)
            {
                throw new Exception($"Your chrome version is {chromeVersion}. Please update your chrome first");
            }
        }

        private static string? GetChromePath(string registryPath, string? valueName = null)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("Your operating system is not supported.");
            try
            {
                return Registry.GetValue(registryPath, valueName, null) as string;
            }
            catch
            {
                return null;
            }
        }
    }
}