using MainCore.Commands.Features.Login;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class LoginTask(
        IDbContextFactory<AppDbContext> contextFactory,
        ChromeManager chromeManager,
        LoginCommand.Handler loginCommand,
        CloseAnnouncementCommand.Handler closeAnnouncementCommand,
        AccecptCookieConsentCommand.Handler accecptCookieConsentCommand,
        DisableContextualHelpCommand.Handler disableContextualHelpCommand
        )
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => nameof(LoginTask).Humanize();
        }

        private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
        {
            var browser = chromeManager.Get(task.AccountId);
            Result result;
            result = await accecptCookieConsentCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;

            var (username, password) = GetLoginInfo(task.AccountId);
            result = await loginCommand.HandleAsync(new(browser, username, password), cancellationToken);
            if (result.IsFailed) return result;

            result = await closeAnnouncementCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;

            result = await disableContextualHelpCommand.HandleAsync(new(browser), cancellationToken);
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