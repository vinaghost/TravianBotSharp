using MainCore.Constraints;

namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class DelayTaskCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            var accountId = command.AccountId;
            
            var delay = context.ByName(accountId, AccountSettingEnums.TaskDelayMin, AccountSettingEnums.TaskDelayMax);
            await Task.Delay(delay, cancellationToken);
        }
    }
}