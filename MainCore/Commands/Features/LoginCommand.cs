namespace MainCore.Commands.Features
{
    [Handler]
    public static partial class LoginCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            if (await LoginParser.IsIngamePage(browser.CurrentPage)) return Result.Ok();

            var (username, password) = GetLoginInfo(command.AccountId, context);

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

        private static (string username, string password) GetLoginInfo(AccountId accountId, AppDbContext context)
        {
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