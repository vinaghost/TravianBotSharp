using MainCore.Queries.Base;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetAccessQuery
    {
        public sealed record Query(AccountId AccountId, bool IgnoreSleepTime = false) : IAccountQuery;

        private static async ValueTask<Result<AccessDto>> HandleAsync(
            Query query,
            IDbContextFactory<AppDbContext> contextFactory,
            VerifyAccessQuery.Handler verifyAccessQuery,
            CancellationToken cancellationToken
            )
        {
            var (accountId, ignoreSleepTime) = query;
            using var context = await contextFactory.CreateDbContextAsync();
            var accesses = context.Accesses
               .Where(x => x.AccountId == accountId.Value)
               .OrderBy(x => x.LastUsed) // get oldest one
               .ToDto()
               .ToList();

            async Task<AccessDto> GetValidAccess(List<AccessDto> accesses)
            {
                foreach (var access in accesses)
                {
                    var result = await verifyAccessQuery.HandleAsync(new(accountId, access), cancellationToken);
                    if (result) return access;
                }
                return null;
            }

            var access = await GetValidAccess(accesses);
            if (access is null) return Stop.AllAccessNotWorking;

            var count = context.Accesses
               .Where(x => x.AccountId == accountId.Value)
               .Count();
            if (count == 1) return access;
            if (ignoreSleepTime) return access;

            var minSleep = context.ByName(accountId, AccountSettingEnums.SleepTimeMin);
            var timeValid = DateTime.Now.AddMinutes(-minSleep);
            if (access.LastUsed > timeValid) return Stop.LackOfAccess;
            return access;
        }
    }
}