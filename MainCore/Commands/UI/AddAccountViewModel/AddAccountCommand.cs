namespace MainCore.Commands.UI.AddAccountViewModel
{
    [Handler]
    public static partial class AddAccountCommand
    {
        public sealed record Command(AccountDto Dto) : ICustomCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IDbContextFactory<AppDbContext> contextFactory, IUseragentManager useragentManager,
            AccountUpdated.Handler accountUpdated,
            CancellationToken cancellationToken
            )
        {
            var dto = command.Dto;
            using var context = await contextFactory.CreateDbContextAsync();
            if (context.Accounts
                .Where(x => x.Username == dto.Username)
                .Where(x => x.Server == dto.Server)
                .Any())
            {
                return Result.Fail("Account is duplicated");
            }

            var account = dto.ToEntity();
            foreach (var access in account.Accesses.Where(access => string.IsNullOrEmpty(access.Useragent)))
            {
                access.Useragent = useragentManager.Get();
            }

            account.Info = new();

            account.Settings = [];
            foreach (var (setting, value) in AppDbContext.AccountDefaultSettings)
            {
                account.Settings.Add(new AccountSetting
                {
                    Setting = setting,
                    Value = value,
                });
            }

            context.Add(account);
            context.SaveChanges();

            await accountUpdated.HandleAsync(new(), cancellationToken);
            return Result.Ok();
        }
    }
}