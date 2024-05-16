using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features
{
    public class LoginCommand : LoginPageCommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public LoginCommand(IDbContextFactory<AppDbContext> contextFactory = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        }

        public async Task<Result> Execute(IChromeBrowser chromeBrowser, AccountId accountId, CancellationToken cancellationToken)
        {
            if (new IsIngamePage().Execute(chromeBrowser)) return Result.Ok();

            var html = chromeBrowser.Html;

            var buttonNode = GetLoginButton(html);
            if (buttonNode is null) return Retry.ButtonNotFound("login");
            var usernameNode = GetUsernameNode(html);
            if (usernameNode is null) return Retry.TextboxNotFound("username");
            var passwordNode = GetPasswordNode(html);
            if (passwordNode is null) return Retry.TextboxNotFound("password");

            var (username, password) = GetLoginInfo(accountId);

            Result result;
            result = await chromeBrowser.InputTextbox(By.XPath(usernameNode.XPath), username);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await chromeBrowser.InputTextbox(By.XPath(passwordNode.XPath), password);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await chromeBrowser.Click(By.XPath(buttonNode.XPath), "dorf", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private static HtmlNode GetUsernameNode(HtmlDocument doc)
        {
            var node = doc.DocumentNode
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "").Equals("name"));
            return node;
        }

        private static HtmlNode GetPasswordNode(HtmlDocument doc)
        {
            var node = doc.DocumentNode
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "").Equals("password"));
            return node;
        }

        private (string, string) GetLoginInfo(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            var username = context.Accounts
                .Where(x => x.Id == accountId.Value)
                .Select(x => x.Username)
                .FirstOrDefault();

            var password = context.Accesses
                .Where(x => x.AccountId == accountId.Value)
                .OrderByDescending(x => x.LastUsed)
                .Select(x => x.Password)
                .FirstOrDefault();
            return (username, password);
        }
    }
}