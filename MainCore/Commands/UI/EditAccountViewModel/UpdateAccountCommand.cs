using MainCore.Constraints;

namespace MainCore.Commands.UI.EditAccountViewModel
{
    [Handler]
    public static partial class UpdateAccountCommand
    {
        public sealed record Command(AccountDto Dto) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            AppDbContext context, IUseragentManager useragentManager, IRxQueue rxQueue,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var dto = command.Dto;

            var account = dto.ToEntity();
            foreach (var access in account.Accesses.Where(access => string.IsNullOrWhiteSpace(access.Useragent)))
            {
                access.Useragent = useragentManager.Get();
            }

            // Remove accesses not present in the DTO
            var existingAccessIds = dto.Accesses.Select(a => a.Id.Value).ToList();
            context.Accesses
                .Where(a => a.AccountId == account.Id && !existingAccessIds.Contains(a.Id))
                .ExecuteDelete();

            context.Update(account);
            context.SaveChanges();

            rxQueue.Enqueue(new AccountsModified());

            return Result.Ok();
        }
    }
}