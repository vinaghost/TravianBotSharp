using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class AccountInfoRepository : IAccountInfoRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public AccountInfoRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public bool IsPlusActive(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            var accountInfo = context.AccountsInfo
                    .FirstOrDefault(x => x.AccountId == accountId.Value);

            if (accountInfo is null) return false;
            return accountInfo.HasPlusAccount;
        }

        public void Update(AccountId accountId, AccountInfoDto dto)
        {
            using var context = _contextFactory.CreateDbContext();

            var dbAccountInfo = context.AccountsInfo
                .Where(x => x.AccountId == accountId.Value)
                .FirstOrDefault();

            if (dbAccountInfo is null)
            {
                var accountInfo = dto.ToEntity(accountId);
                context.Add(accountInfo);
            }
            else
            {
                dto.To(dbAccountInfo);
                context.Update(dbAccountInfo);
            }
            context.SaveChanges();
        }
    }
}