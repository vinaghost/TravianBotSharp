namespace MainCore.Features.Login;

[Handler]
public static partial class AccecptCookieConsentCommand
{
    public sealed record Command(IChromeBrowser Browser) : ICommand
    {
        public string Name => nameof(AccecptCookieConsentCommand).Humanize();
    }

    private static async ValueTask<Result> HandleAsync(Command command)
    {
        var browser = command.Browser;
        Result result;

        var acceptButton = browser.CurrentPage.GetCookieConsentAcceptButton();
        if (await acceptButton.CountAsync() == 0)
        {
            return Result.Ok();
        }

        result = await browser.Click(acceptButton);
        if (result.IsFailed) return result;

        return Result.Ok();
    }
}