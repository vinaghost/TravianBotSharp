namespace MainCore.Commands.Features.Login
{
    [Handler]
    public sealed partial class LoginCommand(IDbContextFactory<AppDbContext> contextFactory)
    {
        public sealed record Command(IChromeBrowser Browser);

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = command.Browser;

            if (await LoginParser.IsIngamePage(browser.CurrentPage)) return Result.Ok();

            var (username, password) = GetLoginInfo(browser.AccountId);

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

        private (string username, string password) GetLoginInfo(AccountId accountId)
        {
            using var context = contextFactory.CreateDbContext();
            var data = context.Accesses
                .Where(x => x.AccountId == accountId.Value)
                .OrderByDescending(x => x.LastUsed)
                .Select(x => new { x.Username, x.Password })
                .FirstOrDefault();

            if (data is null) return ("", "");

            return (data.Username, data.Password);
        }
    }
}