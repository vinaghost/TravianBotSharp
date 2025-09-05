using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Injectio.Attributes;

namespace MainCore.Services
{
    [RegisterSingleton<IBrowserDriverInstaller, GeckoDriverInstaller>]
    public sealed class GeckoDriverInstaller : IBrowserDriverInstaller
    {
        private readonly HttpClient httpClient = new();

        private const string fallbackVersion = "0.36.0"; // GeckoDriver version to download

        public async Task Install()
        {
            CheckPlatform();
            
            var geckoDriverVersion = await GetCurrentDriverVersion();
            
            // Eğer GeckoDriver yoksa veya eskiyse indir
            if (geckoDriverVersion == "0.0.0" || CompareVersions(geckoDriverVersion, fallbackVersion) < 0)
            {
                try
                {
                    await Download(fallbackVersion);
                }
                catch (Exception ex)
                {
                    // GeckoDriver indirme başarısız olursa, uygulama çalışmaya devam etsin
                    // Kullanıcı manuel olarak GeckoDriver'ı PATH'e ekleyebilir
                    Console.WriteLine($"GeckoDriver indirme başarısız: {ex.Message}");
                    Console.WriteLine("Lütfen GeckoDriver'ı manuel olarak PATH'e ekleyin veya uygulama klasörüne koyun.");
                }
            }
        }

        private async Task Download(string version)
        {
            // GitHub API yerine direkt download URL kullan
            var downloadUrl = $"https://github.com/mozilla/geckodriver/releases/download/v{version}/geckodriver-v{version}-win64.zip";
            
            var driverZipResponse = await httpClient.GetAsync(downloadUrl);
            if (!driverZipResponse.IsSuccessStatusCode)
            {
                throw new Exception($"GeckoDriver download failed: {driverZipResponse.StatusCode}");
            }

            var targetPath = GetDriverPath();
            var targetDir = Path.GetDirectoryName(targetPath)!;
            
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            using var zipFileStream = await driverZipResponse.Content.ReadAsStreamAsync();
            using var zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Read);
            
            // GeckoDriver zip dosyasından geckodriver.exe'yi çıkar
            var entry = zipArchive.Entries.First(x => x.Name == "geckodriver.exe");
            using var geckoDriverStream = entry.Open();
            using var geckoDriverWriter = new FileStream(targetPath, FileMode.Create);
            await geckoDriverStream.CopyToAsync(geckoDriverWriter);
        }

        private static void CheckPlatform()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new Exception("GeckoDriver installer only supports Windows");
            }
        }

        private static async Task<string> GetCurrentDriverVersion()
        {
            var driverPath = GetDriverPath();
            if (!File.Exists(driverPath))
            {
                return "0.0.0";
            }

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = driverPath,
                        Arguments = "--version",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                // GeckoDriver version output: "geckodriver 0.36.0"
                var versionMatch = System.Text.RegularExpressions.Regex.Match(output, @"geckodriver (\d+\.\d+\.\d+)");
                return versionMatch.Success ? versionMatch.Groups[1].Value : "0.0.0";
            }
            catch
            {
                return "0.0.0";
            }
        }



        private static string GetDriverPath()
        {
            // Uygulamanın çalıştığı dizini kullan (publish edildiğinde de doğru çalışır)
            var currentDir = Directory.GetCurrentDirectory();
            return Path.Combine(currentDir, "geckodriver.exe");
        }

        private static int CompareVersions(string version1, string version2)
        {
            var v1 = Version.Parse(version1);
            var v2 = Version.Parse(version2);
            return v1.CompareTo(v2);
        }
    }


}
