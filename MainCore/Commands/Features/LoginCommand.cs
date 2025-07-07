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
            var html = browser.Html;
            if (LoginParser.IsIngamePage(html)) return Result.Ok();

            var buttonNode = LoginParser.GetLoginButton(html);
            if (buttonNode is null) return Retry.ButtonNotFound("login");
            var usernameNode = LoginParser.GetUsernameInput(html);
            if (usernameNode is null) return Retry.TextboxNotFound("username");
            var passwordNode = LoginParser.GetPasswordInput(html);
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