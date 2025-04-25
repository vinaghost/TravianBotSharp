namespace MainCore.Commands.UI.Misc
{
    [Handler]
    public static partial class SaveAccountSettingCommand
    {
        public sealed record Command(AccountId AccountId, Dictionary<AccountSettingEnums, int> Settings) : ICustomCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IDbContextFactory<AppDbContext> contextFactory,
            AccountSettingUpdated.Handler accountSettingUpdated,
            CancellationToken cancellationToken
            )
        {
            var (accountId, settings) = command;
            if (settings.Count == 0) return;
            using var context = await contextFactory.CreateDbContextAsync();

            foreach (var setting in settings)
            {
                context.AccountsSetting
                    .Where(x => x.AccountId == accountId.Value)
                    .Where(x => x.Setting == setting.Key)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Value, setting.Value));
            }

            await accountSettingUpdated.HandleAsync(new(accountId), cancellationToken);
        }
    }
}