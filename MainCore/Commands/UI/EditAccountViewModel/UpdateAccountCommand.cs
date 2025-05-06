using MainCore.Constraints;

namespace MainCore.Commands.UI.EditAccountViewModel
{
    [Handler]
    public static partial class UpdateAccountCommand
    {
        public sealed record Command(AccountDto Dto) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context, IUseragentManager useragentManager,
            AccountUpdated.Handler accountUpdated,
            CancellationToken cancellationToken
            )
        {
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

            await accountUpdated.HandleAsync(new(), cancellationToken);
        }
    }
}