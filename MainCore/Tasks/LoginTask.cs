using MainCore.Commands.Features;
using MainCore.Commands.Features.DisableContextualHelp;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class LoginTask
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => "Login";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            LoginCommand.Handler loginCommand,
            ToOptionsPageCommand.Handler toOptionsPageCommand,
            DisableContextualHelpCommand.Handler disableContextualHelpCommand,
            ToDorfCommand.Handler toDorfCommand,
            IChromeBrowser chromeBrowser,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await loginCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;

            var contextualHelpEnable = OptionParser.IsContextualHelpEnable(chromeBrowser.Html);
            if (!contextualHelpEnable) return Result.Ok();

            result = await toOptionsPageCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;
            result = await disableContextualHelpCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;
            result = await toDorfCommand.HandleAsync(new(task.AccountId, 0), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}