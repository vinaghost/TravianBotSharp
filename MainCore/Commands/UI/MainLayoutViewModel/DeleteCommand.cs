using MainCore.Constraints;
using MainCore.Notifications.Behaviors;

namespace MainCore.Commands.UI.MainLayoutViewModel
{
    [Handler]
    [Behaviors(typeof(AccountListUpdatedBehavior<,>))]
    public static partial class DeleteCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var accountId = command.AccountId;

            context.Accounts
                .Where(x => x.Id == accountId.Value)
                .ExecuteDelete();
            return Result.Ok();
        }
    }
}