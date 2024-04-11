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

        public bool IsEnoughCP(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var accountInfo = context.AccountsInfo
                .Where(x => x.AccountId == accountId.Value)
                .FirstOrDefault();
            if (accountInfo is null) return false;

            var villageCount = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Count();

            return accountInfo.MaximumVillage > villageCount;
        }

        public string GetTemplatePath(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var path = context.AccountsInfo
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => x.NewVillageTemplatePath)
                .FirstOrDefault();
            return path;
        }

        public string GetDiscordWebhookUrl(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var path = context.AccountsInfo
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => x.DiscordWebhookUrl)
                .FirstOrDefault();
            return path;
        }

        public void SetTemplatePath(AccountId accountId, string path)
        {
            using var context = _contextFactory.CreateDbContext();

            var dbAccountInfo = context.AccountsInfo
                .Where(x => x.AccountId == accountId.Value)
                .FirstOrDefault();

            if (dbAccountInfo is null)
            {
                var accountInfo = new AccountInfo()
                {
                    AccountId = accountId.Value,
                    NewVillageTemplatePath = path,
                };
                context.Add(accountInfo);
            }
            else
            {
                dbAccountInfo.NewVillageTemplatePath = path;
                context.Update(dbAccountInfo);
            }
            context.SaveChanges();
        }

        public void SetDiscordWebhookUrl(AccountId accountId, string url)
        {
            using var context = _contextFactory.CreateDbContext();

            var dbAccountInfo = context.AccountsInfo
                .Where(x => x.AccountId == accountId.Value)
                .FirstOrDefault();

            if (dbAccountInfo is null)
            {
                var accountInfo = new AccountInfo()
                {
                    AccountId = accountId.Value,
                    DiscordWebhookUrl = url,
                };
                context.Add(accountInfo);
            }
            else
            {
                dbAccountInfo.DiscordWebhookUrl = url;
                context.Update(dbAccountInfo);
            }
            context.SaveChanges();
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