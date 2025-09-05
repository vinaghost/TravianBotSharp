namespace MainCore.Commands.Features
{
    [Handler]
    public static partial class LoginCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IBrowser browser,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            if (LoginParser.IsIngamePage(browser.Html)) return Result.Ok();

            var buttonNode = LoginParser.GetLoginButton(browser.Html);
            if (buttonNode is null) return Retry.ButtonNotFound("login");
            var usernameNode = LoginParser.GetUsernameInput(browser.Html);
            if (usernameNode is null) return Retry.TextboxNotFound("username");
            var passwordNode = LoginParser.GetPasswordInput(browser.Html);
            if (passwordNode is null) return Retry.TextboxNotFound("password");

            var (username, password) = GetLoginInfo(command.AccountId, context);

            Result result;
            result = await browser.Input(By.XPath(usernameNode.XPath), username);
            if (result.IsFailed) return result;
            result = await browser.Input(By.XPath(passwordNode.XPath), password);
            if (result.IsFailed) return result;
            result = await browser.Click(By.XPath(buttonNode.XPath));
            if (result.IsFailed) return result;
            result = await browser.WaitPageChanged("dorf", cancellationToken);
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
