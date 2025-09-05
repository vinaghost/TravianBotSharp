namespace MainCore.Services
{
    [RegisterScoped<ISettingService, SettingService>]
    public class SettingService : ISettingService
    {
        private readonly AppDbContext _context;

        public SettingService(AppDbContext context)
        {
            _context = context;
        }

        public int ByName(VillageId villageId, VillageSettingEnums setting)
        {
            var settingValue = GetSetting.ByNameVillageSettingQuery(_context, villageId.Value, setting);
            return settingValue;
        }

        public int ByName(VillageId villageId, VillageSettingEnums settingMin, VillageSettingEnums settingMax, int multiplier = 1)
        {
            var settings = new List<VillageSettingEnums>
                {
                    settingMin,
                    settingMax,
                };

            var settingValues = _context.VillagesSetting
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
            var settingValue = GetSetting.ByNameAccountSettingQuery(_context, accountId.Value, setting);
            return settingValue;
        }

        public int ByName(AccountId accountId, AccountSettingEnums settingMin, AccountSettingEnums settingMax, int multiplier = 1)
        {
            var settings = new List<AccountSettingEnums>
                {
                    settingMin,
                    settingMax,
                };

            var settingValues = _context.AccountsSetting
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
            var settingValues = _context.VillagesSetting
                   .Where(x => x.VillageId == villageId.Value)
                   .Where(x => settings.Contains(x.Setting))
                   .ToDictionary(x => x.Setting, x => x.Value);
            return settingValues;
        }

        public bool BooleanByName(VillageId villageId, VillageSettingEnums setting)
        {
            var settingValue = GetSetting.ByNameVillageSettingBooleanQuery(_context, villageId.Value, setting);
            return settingValue;
        }

        public bool BooleanByName(AccountId accountId, AccountSettingEnums setting)
        {
            var settingValue = GetSetting.ByNameAccountSettingBooleanQuery(_context, accountId.Value, setting);
            return settingValue;
        }
    }

    public static class GetSetting
    {
        public static readonly Func<AppDbContext, int, VillageSettingEnums, int> ByNameVillageSettingQuery =
            EF.CompileQuery((AppDbContext context, int villageId, VillageSettingEnums setting) =>
                context.VillagesSetting
                    .Where(x => x.VillageId == villageId)
                    .Where(x => x.Setting == setting)
                    .Select(x => x.Value)
                    .FirstOrDefault());

        public static readonly Func<AppDbContext, int, VillageSettingEnums, bool> ByNameVillageSettingBooleanQuery =
            EF.CompileQuery((AppDbContext context, int villageId, VillageSettingEnums setting) =>
                context.VillagesSetting
                    .Where(x => x.VillageId == villageId)
                    .Where(x => x.Setting == setting)
                    .Select(x => x.Value != 0)
                    .FirstOrDefault());

        public static readonly Func<AppDbContext, int, AccountSettingEnums, int> ByNameAccountSettingQuery =
            EF.CompileQuery((AppDbContext context, int accountId, AccountSettingEnums setting) =>
                context.AccountsSetting
                    .Where(x => x.AccountId == accountId)
                    .Where(x => x.Setting == setting)
                    .Select(x => x.Value)
                    .FirstOrDefault());

        public static readonly Func<AppDbContext, int, AccountSettingEnums, bool> ByNameAccountSettingBooleanQuery =
            EF.CompileQuery((AppDbContext context, int accountId, AccountSettingEnums setting) =>
                context.AccountsSetting
                    .Where(x => x.AccountId == accountId)
                    .Where(x => x.Setting == setting)
                    .Select(x => x.Value != 0)
                    .FirstOrDefault());

        public static int ByName(this AppDbContext context, VillageId villageId, VillageSettingEnums setting)
        {
            var settingValue = ByNameVillageSettingQuery(context, villageId.Value, setting);
            return settingValue;
        }

        public static int ByName(this AppDbContext context, VillageId villageId, VillageSettingEnums settingMin, VillageSettingEnums settingMax, int multiplier = 1)
        {
            var settings = new List<VillageSettingEnums>
                {
                    settingMin,
                    settingMax,
                };

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

        public static int ByName(this AppDbContext context, AccountId accountId, AccountSettingEnums setting)
        {
            var settingValue = ByNameAccountSettingQuery(context, accountId.Value, setting);
            return settingValue;
        }

        public static int ByName(this AppDbContext context, AccountId accountId, AccountSettingEnums settingMin, AccountSettingEnums settingMax, int multiplier = 1)
        {
            var settings = new List<AccountSettingEnums>
                {
                    settingMin,
                    settingMax,
                };

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

        public static Dictionary<VillageSettingEnums, int> ByName(this AppDbContext context, VillageId villageId, List<VillageSettingEnums> settings)
        {
            var settingValues = context.VillagesSetting
                   .Where(x => x.VillageId == villageId.Value)
                   .Where(x => settings.Contains(x.Setting))
                   .ToDictionary(x => x.Setting, x => x.Value);
            return settingValues;
        }

        public static bool BooleanByName(this AppDbContext context, VillageId villageId, VillageSettingEnums setting)
        {
            var settingValue = ByNameVillageSettingBooleanQuery(context, villageId.Value, setting);
            return settingValue;
        }

        public static bool BooleanByName(this AppDbContext context, AccountId accountId, AccountSettingEnums setting)
        {
            var settingValue = ByNameAccountSettingBooleanQuery(context, accountId.Value, setting);
            return settingValue;
        }
    }
}
