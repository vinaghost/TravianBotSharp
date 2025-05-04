using MainCore.Commands.Base;

namespace MainCore.Commands.UI.MainLayoutViewModel
{
    [Handler]
    public static partial class DeleteCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            IDbContextFactory<AppDbContext> contextFactory,
            AccountUpdated.Handler accountUpdated,
            CancellationToken cancellationToken
            )
        {
            var accountId = command.AccountId;
            using var context = await contextFactory.CreateDbContextAsync();
            context.Accounts
                .Where(x => x.Id == accountId.Value)
                .ExecuteDelete();
            await accountUpdated.HandleAsync(new(), cancellationToken);
        }
    }
}