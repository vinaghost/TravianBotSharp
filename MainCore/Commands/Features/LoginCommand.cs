using MainCore.Commands.Base;

namespace MainCore.Commands.Features
{
    [Handler]
    public static partial class LoginCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeManager chromeManager,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken)
        {
            var chromeBrowser = chromeManager.Get(command.AccountId);

            var html = chromeBrowser.Html;
            if (LoginParser.IsIngamePage(html)) return Result.Ok();

            var buttonNode = LoginParser.GetLoginButton(html);
            if (buttonNode is null) return Retry.ButtonNotFound("login");
            var usernameNode = LoginParser.GetUsernameInput(html);
            if (usernameNode is null) return Retry.TextboxNotFound("username");
            var passwordNode = LoginParser.GetPasswordInput(html);
            if (passwordNode is null) return Retry.TextboxNotFound("password");

            var (username, password) = GetLoginInfo(command.AccountId, contextFactory);

            Result result;
            result = await chromeBrowser.Input(By.XPath(usernameNode.XPath), username);
            if (result.IsFailed) return result;
            result = await chromeBrowser.Input(By.XPath(passwordNode.XPath), password);
            if (result.IsFailed) return result;
            result = await chromeBrowser.Click(By.XPath(buttonNode.XPath));
            if (result.IsFailed) return result;
            result = await chromeBrowser.WaitPageChanged("dorf", cancellationToken);

            return Result.Ok();
        }

        private static (string username, string password) GetLoginInfo(AccountId accountId, IDbContextFactory<AppDbContext> contextFactory)
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