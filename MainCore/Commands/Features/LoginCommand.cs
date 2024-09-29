using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class LoginCommand(DataService dataService, IDbContextFactory<AppDbContext> contextFactory) : CommandBase(dataService)
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

        public override async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var accountId = _dataService.AccountId;
            var chromeBrowser = _dataService.ChromeBrowser;

            var html = chromeBrowser.Html;
            if (LoginParser.IsIngamePage(html)) return Result.Ok();

            var buttonNode = LoginParser.GetLoginButton(html);
            if (buttonNode is null) return Retry.ButtonNotFound("login");
            var usernameNode = LoginParser.GetUsernameInput(html);
            if (usernameNode is null) return Retry.TextboxNotFound("username");
            var passwordNode = LoginParser.GetPasswordInput(html);
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