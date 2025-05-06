using MainCore.Constraints;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetValidAccessQuery
    {
        public sealed record Query(AccountId AccountId, bool IgnoreSleepTime = false) : IAccountQuery;

        private static async ValueTask<Result<AccessDto>> HandleAsync(
            Query query,
            GetAccessesQuery.Handler getAccessesQuery,
            VerifyAccessQuery.Handler verifyAccessQuery,
            ISettingService settingService,
            CancellationToken cancellationToken
            )
        {
            var (accountId, ignoreSleepTime) = query;

            var accesses = await getAccessesQuery.HandleAsync(new(accountId), cancellationToken);

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

            if (accesses.Count == 1) return access;
            if (ignoreSleepTime) return access;

            var minSleep = settingService.ByName(accountId, AccountSettingEnums.SleepTimeMin);
            var timeValid = DateTime.Now.AddMinutes(-minSleep);
            if (access.LastUsed > timeValid) return Stop.LackOfAccess;
            return access;
        }
    }
}