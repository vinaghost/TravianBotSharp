using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Commands.Queries
{
    public class GetAccountSetting
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public GetAccountSetting(IDbContextFactory<AppDbContext> contextFactory = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        }

        public int ByName(AccountId accountId, AccountSettingEnums setting)
        {
            using var context = _contextFactory.CreateDbContext();
            var settingValue = context.AccountsSetting
                   .Where(x => x.AccountId == accountId.Value)
                   .Where(x => x.Setting == setting)
                   .Select(x => x.Value)
                   .FirstOrDefault();
            return settingValue;
        }

        public int ByName(AccountId accountId, AccountSettingEnums settingMin, AccountSettingEnums settingMax, int multiplier = 1)
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

        public bool BooleanByName(AccountId accountId, AccountSettingEnums setting)
        {
            using var context = _contextFactory.CreateDbContext();
            var settingValue = context.AccountsSetting
                   .Where(x => x.AccountId == accountId.Value)
                   .Where(x => x.Setting == setting)
                   .Select(x => x.Value != 0)
                   .FirstOrDefault();

            return settingValue;
        }
    }
}