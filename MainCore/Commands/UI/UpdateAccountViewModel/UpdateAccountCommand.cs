namespace MainCore.Commands.UI.UpdateAccountViewModel
{
    [Handler]
    public static partial class UpdateAccountCommand
    {
        public sealed record Command(AccountDto Dto) : ICustomCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IDbContextFactory<AppDbContext> contextFactory, IUseragentManager useragentManager,
            AccountUpdated.Handler accountUpdated,
            CancellationToken cancellationToken
            )
        {
            var dto = command.Dto;
            using var context = await contextFactory.CreateDbContextAsync();

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