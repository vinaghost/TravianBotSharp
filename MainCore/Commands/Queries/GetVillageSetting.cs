using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Commands.Queries
{
    public class GetVillageSetting
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public GetVillageSetting(IDbContextFactory<AppDbContext> contextFactory = null)
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
    }
}