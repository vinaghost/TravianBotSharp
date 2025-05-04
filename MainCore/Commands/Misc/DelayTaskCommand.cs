using MainCore.Commands.Base;

namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class DelayTaskCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken)
        {
            var accountId = command.AccountId;
            using var context = await contextFactory.CreateDbContextAsync();
            var delay = context.ByName(accountId, AccountSettingEnums.TaskDelayMin, AccountSettingEnums.TaskDelayMax);
            await Task.Delay(delay, cancellationToken);
        }
    }
}