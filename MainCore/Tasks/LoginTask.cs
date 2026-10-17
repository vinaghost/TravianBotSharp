using MainCore.Commands.Features.Login;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class LoginTask(
        IChromeBrowser browser,
        LoginCommand.Handler loginCommand,
        AccecptCookieConsentCommand.Handler accecptCookieConsentCommand,
        DisableContextualHelpCommand.Handler disableContextualHelpCommand
        )
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => "Login";
        }

        private async ValueTask<Result> HandleAsync(
#pragma warning disable S1172 // Unused method parameters should be removed
#pragma warning disable IDE0060 // Remove unused parameter
            Task task,
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore S1172 // Unused method parameters should be removed
            CancellationToken cancellationToken
           )
        {
            await accecptCookieConsentCommand.HandleAsync(new(browser), cancellationToken);

            Result result;
            result = await loginCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;

            result = await disableContextualHelpCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}