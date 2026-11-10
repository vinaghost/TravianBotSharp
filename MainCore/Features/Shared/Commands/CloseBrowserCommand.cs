namespace MainCore.Features.Shared.Commands;

[Handler]
public static partial class CloseBrowserCommand
{
    public sealed record Command(IChromeBrowser Browser) : IBrowserCommand
    {
        public string Name => $"{nameof(CloseBrowserCommand)}";
    }

    private static async ValueTask HandleAsync(Command command)
    {
        await command.Browser.Shutdown();
    }
}