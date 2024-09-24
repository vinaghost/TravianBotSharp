namespace MainCore.Commands.Queries
{
    public class GetSetting(IDbContextFactory<AppDbContext> contextFactory = null)
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();

        private static readonly Func<AppDbContext, VillageId, VillageSettingEnums, int> ByNameVillageSettingQuery =
            EF.CompileQuery((AppDbContext context, VillageId villageId, VillageSettingEnums setting) =>
                context.VillagesSetting
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Setting == setting)
                    .Select(x => x.Value)
                    .FirstOrDefault());

        private static readonly Func<AppDbContext, VillageId, VillageSettingEnums, bool> ByNameVillageSettingBooleanQuery =
            EF.CompileQuery((AppDbContext context, VillageId villageId, VillageSettingEnums setting) =>
                context.VillagesSetting
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Setting == setting)
                    .Select(x => x.Value != 0)
                    .FirstOrDefault());

        private static readonly Func<AppDbContext, AccountId, AccountSettingEnums, int> ByNameAccountSettingQuery =
            EF.CompileQuery((AppDbContext context, AccountId accountId, AccountSettingEnums setting) =>
                context.AccountsSetting
                    .Where(x => x.AccountId == accountId.Value)
                    .Where(x => x.Setting == setting)
                    .Select(x => x.Value)
                    .FirstOrDefault());

        private static readonly Func<AppDbContext, AccountId, AccountSettingEnums, bool> ByNameAccountSettingBooleanQuery =
            EF.CompileQuery((AppDbContext context, AccountId accountId, AccountSettingEnums setting) =>
                context.AccountsSetting
                    .Where(x => x.AccountId == accountId.Value)
                    .Where(x => x.Setting == setting)
                    .Select(x => x.Value != 0)
                    .FirstOrDefault());

        public int ByName(VillageId villageId, VillageSettingEnums setting)
        {
            using var context = _contextFactory.CreateDbContext();
            var settingValue = ByNameVillageSettingQuery(context, villageId, setting);
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

        public int ByName(AccountId accountId, AccountSettingEnums setting)
        {
            using var context = _contextFactory.CreateDbContext();
            var settingValue = ByNameAccountSettingQuery(context, accountId, setting);
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

            if (settingValues.Count != 2) return 1000;
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
            var settingValue = ByNameVillageSettingBooleanQuery(context, villageId, setting);
            return settingValue;
        }

        public bool BooleanByName(AccountId accountId, AccountSettingEnums setting)
        {
            using var context = _contextFactory.CreateDbContext();
            var settingValue = ByNameAccountSettingBooleanQuery(context, accountId, setting);
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