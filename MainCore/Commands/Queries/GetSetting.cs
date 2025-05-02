namespace MainCore.Commands.Queries
{
    public static class GetSetting
    {
        private static readonly Func<AppDbContext, int, VillageSettingEnums, int> ByNameVillageSettingQuery =
            EF.CompileQuery((AppDbContext context, int villageId, VillageSettingEnums setting) =>
                context.VillagesSetting
                    .Where(x => x.VillageId == villageId)
                    .Where(x => x.Setting == setting)
                    .Select(x => x.Value)
                    .FirstOrDefault());

        private static readonly Func<AppDbContext, int, VillageSettingEnums, bool> ByNameVillageSettingBooleanQuery =
            EF.CompileQuery((AppDbContext context, int villageId, VillageSettingEnums setting) =>
                context.VillagesSetting
                    .Where(x => x.VillageId == villageId)
                    .Where(x => x.Setting == setting)
                    .Select(x => x.Value != 0)
                    .FirstOrDefault());

        private static readonly Func<AppDbContext, int, AccountSettingEnums, int> ByNameAccountSettingQuery =
            EF.CompileQuery((AppDbContext context, int accountId, AccountSettingEnums setting) =>
                context.AccountsSetting
                    .Where(x => x.AccountId == accountId)
                    .Where(x => x.Setting == setting)
                    .Select(x => x.Value)
                    .FirstOrDefault());

        private static readonly Func<AppDbContext, int, AccountSettingEnums, bool> ByNameAccountSettingBooleanQuery =
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