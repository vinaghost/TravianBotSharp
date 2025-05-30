﻿using MainCore.Constraints;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetAccessesQuery
    {
        public sealed record Query(AccountId AccountId) : IAccountQuery;

        private static async ValueTask<List<AccessDto>> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken
        )
        {
            await Task.CompletedTask;
            var accountId = query.AccountId;

            var accesses = context.Accesses
               .Where(x => x.AccountId == accountId.Value)
               .OrderBy(x => x.LastUsed) // get oldest one
               .ToDto()
               .ToList();
            return accesses;
        }
    }
}