namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class DelayTaskCommand
    {
        public sealed record Command(AccountId AccountId) : ICustomCommand;

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