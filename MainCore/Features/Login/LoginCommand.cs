namespace MainCore.Features.Login;

[Handler]
public static partial class LoginCommand
{
    public sealed record Command(IChromeBrowser Browser, string Username, string Password) : ICommand
    {
        public string Name => nameof(LoginCommand).Humanize();
    }

    private static async ValueTask<Result> HandleAsync(Command command)
    {
        var (browser, username, password) = command;

        var page = browser.CurrentPage;
        if (await page.IsIngamePage()) return Result.Ok();

        Result result;

        result = await browser.Input(page.GetUsernameInput(), username);
        if (result.IsFailed) return result;

        result = await browser.Input(page.GetPasswordInput(), password);
        if (result.IsFailed) return result;

        result = await browser.Click(page.GetLoginButton());
        if (result.IsFailed) return result;

        result = await browser.WaitPageChanged("dorf");
        if (result.IsFailed) return result;

        return Result.Ok();
    }
}