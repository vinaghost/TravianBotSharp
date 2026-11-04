namespace MainCore.Commands.Features.Login
{
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

            if (await LoginParser.IsIngamePage(browser.CurrentPage)) return Result.Ok();

            Result result;

            result = await browser.Input(LoginParser.GetUsernameInput(browser.CurrentPage), username);
            if (result.IsFailed) return result;

            result = await browser.Input(LoginParser.GetPasswordInput(browser.CurrentPage), password);
            if (result.IsFailed) return result;

            result = await browser.Click(LoginParser.GetLoginButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged("dorf");
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}