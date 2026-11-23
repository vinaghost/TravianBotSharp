namespace MainCore.Features.InstantUpgrade;

[Handler]
public static partial class InstantUpgradeCommand
{
    public sealed record Command(IChromeBrowser Browser) : ICommand
    {
        public string Name => nameof(InstantUpgradeCommand).Humanize();
    }

    private static async ValueTask<Result> HandleAsync(Command command)
    {
        var browser = command.Browser;

        var completeButton = browser.CurrentPage.GetCompleteButton();
        var result = await browser.Click(completeButton);
        if (result.IsFailed) return result;

        var confirmButton = browser.CurrentPage.GetConfirmButton();
        result = await browser.Click(confirmButton);
        if (result.IsFailed) return result;

        var pageBody = browser.CurrentPage.Locator("body");
        result = await browser.Wait(pageBody, condition: "node => node.querySelector('div.buildingList') === null");
        if (result.IsFailed) return result;
        return Result.Ok();
    }
}