namespace MainCore.Commands.UI.AccountSettingViewModel
{
    [Handler]
    public static partial class GetSettingQuery
    {
        public sealed record Query(AccountId AccountId) : ICustomQuery;

        private static async ValueTask<Dictionary<AccountSettingEnums, int>> HandleAsync(
            Query query,
            IDbContextFactory<AppDbContext> contextFactory,
            CancellationToken cancellationToken
            )
        {
            var accountId = query.AccountId;
            using var context = await contextFactory.CreateDbContextAsync();

            var settings = context.AccountsSetting
               .Where(x => x.AccountId == accountId.Value)
               .ToDictionary(x => x.Setting, x => x.Value);
            return settings;
        }
    }
}