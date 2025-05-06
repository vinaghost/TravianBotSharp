using MainCore.Constraints;

namespace MainCore.Commands.UI.MainLayoutViewModel
{
    [Handler]
    public static partial class DeleteCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            AccountUpdated.Handler accountUpdated,
            CancellationToken cancellationToken
            )
        {
            var accountId = command.AccountId;

            context.Accounts
                .Where(x => x.Id == accountId.Value)
                .ExecuteDelete();
            await accountUpdated.HandleAsync(new(), cancellationToken);
        }
    }
}