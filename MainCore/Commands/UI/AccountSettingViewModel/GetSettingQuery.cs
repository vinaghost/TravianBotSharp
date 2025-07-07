namespace MainCore.Commands.UI.AccountSettingViewModel
{
    [Handler]
    public static partial class GetSettingQuery
    {
        public sealed record Query(AccountId AccountId) : IAccountQuery;

        private static async ValueTask<Dictionary<AccountSettingEnums, int>> HandleAsync(
            Query query,
            AppDbContext context
            )
        {
            await Task.CompletedTask;
            var accountId = query.AccountId;

            var settings = context.AccountsSetting
               .Where(x => x.AccountId == accountId.Value)
               .ToDictionary(x => x.Setting, x => x.Value);
            return settings;
        }
    }
}