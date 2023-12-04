using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class AccountSettingRepository : IAccountSettingRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public AccountSettingRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public int GetByName(AccountId accountId, AccountSettingEnums setting)
        {
            using var context = _contextFactory.CreateDbContext();
            var settingValue = context.AccountsSetting
                   .Where(x => x.AccountId == accountId.Value)
                   .Where(x => x.Setting == setting)
                   .Select(x => x.Value)
                   .FirstOrDefault();
            return settingValue;
        }

        public int GetByName(AccountId accountId, AccountSettingEnums settingMin, AccountSettingEnums settingMax, int multiplier = 1)
        {
            var settings = new List<AccountSettingEnums>
            {
                settingMin,
                settingMax,
            };

            using var context = _contextFactory.CreateDbContext();
            var settingValues = context.AccountsSetting
                   .Where(x => x.AccountId == accountId.Value)
                   .Where(x => settings.Contains(x.Setting))
                   .Select(x => x.Value)
                   .ToList();

            if (settingValues.Count != 2) return 0;
            var min = settingValues.Min();
            var max = settingValues.Max();

            return Random.Shared.Next(min * multiplier, max * multiplier);
        }

        public bool GetBooleanByName(AccountId accountId, AccountSettingEnums setting)
        {
            using var context = _contextFactory.CreateDbContext();
            var settingValue = context.AccountsSetting
                   .Where(x => x.AccountId == accountId.Value)
                   .Where(x => x.Setting == setting)
                   .Select(x => x.Value != 0)
                   .FirstOrDefault();

            return settingValue;
        }

        public void Update(AccountId accountId, Dictionary<AccountSettingEnums, int> settings)
        {
            using var context = _contextFactory.CreateDbContext();

            foreach (var setting in settings)
            {
                context.AccountsSetting
                    .Where(x => x.AccountId == accountId.Value)
                    .Where(x => x.Setting == setting.Key)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Value, setting.Value));
            }
        }

        public Dictionary<AccountSettingEnums, int> Get(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var settings = context.AccountsSetting
                .Where(x => x.AccountId == accountId.Value)
                .ToDictionary(x => x.Setting, x => x.Value);
            return settings;
        }
    }
}