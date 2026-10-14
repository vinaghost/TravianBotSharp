using Avalonia;

using MainCore;

namespace AvaloniaUI
{
    internal static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // MainCore writes TBS.db and logs relative to the working directory.
            // When the app is started from Finder the working directory is "/",
            // so point it at the app data folder first.
            Directory.CreateDirectory(AppPaths.Root);
            Directory.SetCurrentDirectory(AppPaths.Root);

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
