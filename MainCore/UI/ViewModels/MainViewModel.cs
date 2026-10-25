using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

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
            if (OperatingSystem.IsWindows())
            {
                chromeVersion = GetWindowsChromeVersion();
            }
            else if (OperatingSystem.IsMacOS())
            {
                chromeVersion = GetMacChromeVersion();
            }
            else
            {
                throw new PlatformNotSupportedException("Your operating system is not supported.");
            }

            if (chromeVersion.Major <= 114)
            {
                throw new Exception($"Your chrome version is {chromeVersion}. Please update your chrome first");
            }
        }

        [SupportedOSPlatform("windows")]
        private static Version GetWindowsChromeVersion()
        {
            string chromePath = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe", null, null) as string ?? throw new Exception("Google Chrome not found in registry");
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(chromePath);
            if (fileVersionInfo.FileVersion is null)
            {
                throw new Exception("Failed to get chrome version");
            }
            return new Version(fileVersionInfo.FileVersion);
        }

        private static Version GetMacChromeVersion()
        {
            // Playwright's "chrome" channel launches /Applications/Google Chrome.app
            const string infoPlistPath = "/Applications/Google Chrome.app/Contents/Info.plist";
            if (!File.Exists(infoPlistPath))
            {
                throw new Exception("Google Chrome not found. Please install Google Chrome into /Applications");
            }

            var plist = File.ReadAllText(infoPlistPath);
            var match = Regex.Match(plist, @"<key>CFBundleShortVersionString</key>\s*<string>([^<]+)</string>");
            if (!match.Success || !Version.TryParse(match.Groups[1].Value, out var version))
            {
                throw new Exception("Failed to get chrome version");
            }
            return version;
        }
    }
}