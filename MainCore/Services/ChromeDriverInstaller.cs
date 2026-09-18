using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;

#nullable disable

namespace MainCore.Services
{
    [RegisterSingleton<IChromeDriverInstaller, ChromeDriverInstaller>]
    public sealed class ChromeDriverInstaller : IChromeDriverInstaller
    {
        public async Task Install()
        {
            CheckPlatform();
            var chromeVersion = await GetChromeVersion();

            if (chromeVersion.Major <= 114)
            {
                throw new Exception($"Your chrome version is {chromeVersion}. Please update your chrome first");
            }
        }

        private static void CheckPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            { return; }
            throw new PlatformNotSupportedException("Your operating system is not supported.");
        }

        private static async Task<Version> GetChromeVersion()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string chromePath = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe", null, null);
                if (chromePath == null)
                {
                    throw new Exception("Google Chrome not found in registry");
                }

                var fileVersionInfo = FileVersionInfo.GetVersionInfo(chromePath);
                return new Version(fileVersionInfo.FileVersion);
            }
            else
            {
                throw new PlatformNotSupportedException("Your operating system is not supported.");
            }
        }
    }
}