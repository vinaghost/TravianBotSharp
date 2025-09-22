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
            if (LoginParser.IsIngamePage(browser.Html)) return Result.Ok();

            var (username, password) = GetLoginInfo(command.AccountId, context);

            Result result;

            var (_, isFailed, element, errors) = await browser.GetElement(doc => LoginParser.GetUsernameInput(doc), cancellationToken);
            if (isFailed) return Result.Fail(errors);
            result = await browser.Input(element, username, cancellationToken);
            if (result.IsFailed) return result;

            (_, isFailed, element, errors) = await browser.GetElement(doc => LoginParser.GetPasswordInput(doc), cancellationToken);
            if (isFailed) return Result.Fail(errors);
            result = await browser.Input(element, password, cancellationToken);
            if (result.IsFailed) return result;

            (_, isFailed, element, errors) = await browser.GetElement(doc => LoginParser.GetLoginButton(doc), cancellationToken);
            if (isFailed) return Result.Fail(errors);
            result = await browser.Click(element, cancellationToken);
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