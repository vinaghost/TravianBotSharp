using MainCore.Constraints;
using MainCore.Notifications.Behaviors;

namespace MainCore.Commands.UI.EditAccountViewModel
{
    [Handler]
    [Behaviors(typeof(AccountListUpdatedBehavior<,>))]
    public static partial class UpdateAccountCommand
    {
        public sealed record Command(AccountDto Dto) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context, IUseragentManager useragentManager,
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
        }
    }
}