namespace MainCore.Commands.Queries
{
    public class GetSetting
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public GetSetting(IDbContextFactory<AppDbContext> contextFactory = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        }

        public int ByName(VillageId villageId, VillageSettingEnums setting)
        {
            using var context = _contextFactory.CreateDbContext();
            var settingValue = context.VillagesSetting
                   .Where(x => x.VillageId == villageId.Value)
                   .Where(x => x.Setting == setting)
                   .Select(x => x.Value)
                   .FirstOrDefault();
            return settingValue;
        }

        public int ByName(VillageId villageId, VillageSettingEnums settingMin, VillageSettingEnums settingMax, int multiplier = 1)
        {
            var settings = new List<VillageSettingEnums>
            {
                settingMin,
                settingMax,
            };
            using var context = _contextFactory.CreateDbContext();
            var settingValues = context.VillagesSetting
                   .Where(x => x.VillageId == villageId.Value)
                   .Where(x => settings.Contains(x.Setting))
                   .Select(x => x.Value)
                   .ToList();
            if (settingValues.Count != 2) return 0;
            var min = settingValues.Min();
            var max = settingValues.Max();
            return Random.Shared.Next(min * multiplier, max * multiplier);
        }

        public Dictionary<VillageSettingEnums, int> ByName(VillageId villageId, List<VillageSettingEnums> settings)
        {
            using var context = _contextFactory.CreateDbContext();
            var settingValues = context.VillagesSetting
                   .Where(x => x.VillageId == villageId.Value)
                   .Where(x => settings.Contains(x.Setting))
                   .ToDictionary(x => x.Setting, x => x.Value);
            return settingValues;
        }

        public bool BooleanByName(VillageId villageId, VillageSettingEnums setting)
        {
            using var context = _contextFactory.CreateDbContext();
            var settingValue = context.VillagesSetting
                   .Where(x => x.VillageId == villageId.Value)
                   .Where(x => x.Setting == setting)
                   .Select(x => x.Value != 0)
                   .FirstOrDefault();

            return settingValue;
        }

        public Dictionary<AccountSettingEnums, int> Get(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var settings = context.AccountsSetting
                .Where(x => x.AccountId == accountId.Value)
                .ToDictionary(x => x.Setting, x => x.Value);
            return settings;
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

        public Dictionary<VillageSettingEnums, int> Get(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            var settings = context.VillagesSetting
                .Where(x => x.VillageId == villageId.Value)
                .ToDictionary(x => x.Setting, x => x.Value);
            return settings;
        }
    }
}