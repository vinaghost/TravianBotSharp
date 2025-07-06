namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class DelayClickCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            ISettingService settingService,
            CancellationToken cancellationToken)
        {
            var accountId = command.AccountId;

            var delay = settingService.ByName(accountId, AccountSettingEnums.ClickDelayMin, AccountSettingEnums.ClickDelayMax);
            await Task.Delay(delay, cancellationToken);
        }
    }
}