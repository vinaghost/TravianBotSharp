namespace MainCore.Features.UseHeroItem;

[Handler]
public static partial class ToHeroInventoryCommand
{
    public sealed record Command(IChromeBrowser Browser) : ICommand
    {
        public string Name => nameof(ToHeroInventoryCommand).Humanize();
    }

    private static async ValueTask<Result> HandleAsync(Command command)
    {
        var browser = command.Browser;
        var result = await browser.Click(browser.CurrentPage.GetHeroAvatar());
        if (result.IsFailed) return result;

        result = await browser.Wait(browser.CurrentPage.GetInventoryPageWrapper());
        if (result.IsFailed) return result;

        result = await browser.Wait(browser.CurrentPage.GetInventoryPageWrapper(), condition: "node => node.classList.contains('loading')");
        if (result.IsFailed) return result;

        return Result.Ok();
    }
}