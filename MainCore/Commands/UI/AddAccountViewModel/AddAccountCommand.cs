using MainCore.Constraints;
using MainCore.Notifications.Behaviors;

namespace MainCore.Commands.UI.AddAccountViewModel
{
    [Handler]
    [Behaviors(typeof(AccountListUpdatedBehavior<,>))]
    public static partial class AddAccountCommand
    {
        public sealed record Command(AccountDto Dto) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            AppDbContext context, IUseragentManager useragentManager,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var dto = command.Dto;

            if (context.Accounts
                .Where(x => x.Username == dto.Username)
                .Where(x => x.Server == dto.Server)
                .Any())
            {
                return AccountDuplicate.Error;
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

            return Result.Ok();
        }
    }
}