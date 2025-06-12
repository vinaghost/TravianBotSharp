using MainCore.Constraints;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetTelegramSettingQuery
    {
        public sealed record Query(AccountId AccountId) : IAccountQuery;

        private static async ValueTask<TelegramSetting?> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = query.AccountId;
            var setting = context.TelegramSettings
                .FirstOrDefault(x => x.AccountId == accountId.Value);
            return setting;
        }
    }
}
