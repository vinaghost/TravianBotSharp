namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class DelayTaskCommand
    {
        public sealed record Command(AccountId AccountId) : ICustomCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IGetSetting getSetting,
            CancellationToken cancellationToken)
        {
            var accountId = command.AccountId;
            var delay = getSetting.ByName(accountId, AccountSettingEnums.TaskDelayMin, AccountSettingEnums.TaskDelayMax);

            await Task.Delay(delay, cancellationToken);
        }
    }
}