namespace MainCore.Commands.UI.MainLayoutViewModel
{
    [Handler]
    public static partial class DeleteCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            AppDbContext context, IRxQueue rxQueue,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var accountId = command.AccountId;

            context.Accounts
                .Where(x => x.Id == accountId.Value)
                .ExecuteDelete();

            rxQueue.Enqueue(new AccountsModified());

            return Result.Ok();
        }
    }
}