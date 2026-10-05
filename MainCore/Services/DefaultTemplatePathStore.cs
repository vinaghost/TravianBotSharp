namespace MainCore.Services
{
    [RegisterSingleton<DefaultTemplatePathStore>]
    public sealed class DefaultTemplatePathStore
    {
        private const string AccountSettingsKey = "DefaultAccountSettingsPath";
        private const string VillageSettingsKey = "DefaultVillageSettingsPath";
        private const string BuildingListKey = "DefaultBuildingListPath";

        private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "default-templates.csv");
        private readonly object _sync = new();

        public DefaultTemplatePaths Get()
        {
            lock (_sync)
            {
                var settings = LoadSettings();
                return new DefaultTemplatePaths(
                    GetValue(settings, AccountSettingsKey),
                    GetValue(settings, VillageSettingsKey),
                    GetValue(settings, BuildingListKey));
            }
        }

        public void SetAccountSettingsPath(string path)
        {
            SetValue(AccountSettingsKey, path);
        }

        public void SetVillageSettingsPath(string path)
        {
            SetValue(VillageSettingsKey, path);
        }

        public void SetBuildingListPath(string path)
        {
            SetValue(BuildingListKey, path);
        }

        public void ClearAccountSettingsPath()
        {
            SetValue(AccountSettingsKey, string.Empty);
        }

        public void ClearVillageSettingsPath()
        {
            SetValue(VillageSettingsKey, string.Empty);
        }

        public void ClearBuildingListPath()
        {
            SetValue(BuildingListKey, string.Empty);
        }

        private static string GetValue(Dictionary<string, string> settings, string key)
        {
            if (!settings.TryGetValue(key, out var value)) return string.Empty;
            return value;
        }

        private void SetValue(string key, string value)
        {
            lock (_sync)
            {
                var settings = LoadSettings();
                settings[key] = value;
                SaveSettings(settings);
            }
        }

        private Dictionary<string, string> LoadSettings()
        {
            var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(_filePath)) return settings;

            foreach (var line in File.ReadLines(_filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var separatorIndex = line.IndexOf(',');
                if (separatorIndex <= 0) continue;

                var key = line[..separatorIndex].Trim();
                if (string.IsNullOrEmpty(key)) continue;

                var value = separatorIndex >= line.Length - 1 ? string.Empty : line[(separatorIndex + 1)..];

                if (key != AccountSettingsKey && key != VillageSettingsKey && key != BuildingListKey) continue;

                settings[key] = value;
            }

            return settings;
        }

        private void SaveSettings(Dictionary<string, string> settings)
        {
            var folder = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using var writer = new StreamWriter(_filePath, false);
            writer.WriteLine($"{AccountSettingsKey},{GetValue(settings, AccountSettingsKey)}");
            writer.WriteLine($"{VillageSettingsKey},{GetValue(settings, VillageSettingsKey)}");
            writer.WriteLine($"{BuildingListKey},{GetValue(settings, BuildingListKey)}");
        }
    }
}
