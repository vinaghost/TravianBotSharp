namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class DelayClickCommand
    {
        public sealed record Command(AccountId AccountId) : ICustomCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IGetSetting getSetting,
            CancellationToken cancellationToken)
        {
            var accountId = command.AccountId;
            var delay = getSetting.ByName(accountId, AccountSettingEnums.ClickDelayMin, AccountSettingEnums.ClickDelayMax);

            await Task.Delay(delay, cancellationToken);
        }
    }
}