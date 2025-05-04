using MainCore.Commands.Base;

namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class DelayClickCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken)
        {
            var accountId = command.AccountId;
            using var context = await contextFactory.CreateDbContextAsync();
            var delay = context.ByName(accountId, AccountSettingEnums.ClickDelayMin, AccountSettingEnums.ClickDelayMax);
            await Task.Delay(delay, cancellationToken);
        }
    }
}