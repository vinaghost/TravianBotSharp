namespace MainCore
{
    public static class AppPaths
    {
        /// <summary>
        /// Folder that holds the database, logs, browser profiles and screenshots.
        /// On Windows this stays next to the executable; on macOS the app bundle is not
        /// a good place for user data, so it lives in ~/Library/Application Support.
        /// </summary>
        public static string Root { get; } = GetRoot();

        private static string GetRoot()
        {
            if (OperatingSystem.IsMacOS())
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(home, "Library", "Application Support", "TravianBotSharp");
            }
            return AppContext.BaseDirectory;
        }
    }
}
