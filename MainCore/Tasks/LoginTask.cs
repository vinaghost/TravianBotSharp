using MainCore.Commands.Features.Login;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class LoginTask(
        LoginCommand.Handler loginCommand,
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
            Result result;
            result = await accecptCookieConsentCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;

            result = await loginCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;

            result = await disableContextualHelpCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}