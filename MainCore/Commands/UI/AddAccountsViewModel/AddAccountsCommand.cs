using MainCore.Constraints;

namespace MainCore.Commands.UI.AddAccountsViewModel
{
    [Handler]
    public static partial class AddAccountsCommand
    {
        public sealed record Command(List<AccountDto> Dtos) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            AppDbContext context, IUseragentManager useragentManager, IRxQueue rxQueue,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var existAccounts = context.Accounts
                .ToDto()
                .ToList();

            var dtos = command.Dtos
                .Where(dto => !existAccounts.Exists(x => x.Username == dto.Username && x.Server == dto.Server))
                .ToList();

            if (dtos.Count == 0)
            {
                return Result.Fail("All accounts are duplicated");
            }
            var accounts = dtos
                .Select(x => x.ToEntity());

            foreach (var access in accounts.SelectMany(x => x.Accesses).Where(access => string.IsNullOrEmpty(access.Useragent)))
            {
                access.Useragent = useragentManager.Get();
            }

            foreach (var account in accounts)
            {
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
            }

            context.AddRange(accounts);
            context.SaveChanges();

            rxQueue.Enqueue(new AccountsModified());

            return Result.Ok();
        }
    }
}