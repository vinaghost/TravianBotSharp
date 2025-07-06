namespace MainCore.Commands.UI.Misc
{
    [Handler]
    public static partial class SaveAccountSettingCommand
    {
        public sealed record Command(AccountId AccountId, Dictionary<AccountSettingEnums, int> Settings) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var (accountId, settings) = command;
            if (settings.Count == 0) return;

            foreach (var setting in settings)
            {
                context.AccountsSetting
                    .Where(x => x.AccountId == accountId.Value)
                    .Where(x => x.Setting == setting.Key)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Value, setting.Value));
            }
        }
    }
}