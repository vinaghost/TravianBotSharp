using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class VillageSettingRepository : IVillageSettingRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public VillageSettingRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public int GetByName(VillageId villageId, VillageSettingEnums setting)
        {
            using var context = _contextFactory.CreateDbContext();
            var settingValue = context.VillagesSetting
                   .Where(x => x.VillageId == villageId.Value)
                   .Where(x => x.Setting == setting)
                   .Select(x => x.Value)
                   .FirstOrDefault();
            return settingValue;
        }

        public int GetByName(VillageId villageId, VillageSettingEnums settingMin, VillageSettingEnums settingMax, int multiplier = 1)
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

        public Dictionary<VillageSettingEnums, int> GetByName(VillageId villageId, List<VillageSettingEnums> settings)
        {
            using var context = _contextFactory.CreateDbContext();
            var settingValues = context.VillagesSetting
                   .Where(x => x.VillageId == villageId.Value)
                   .Where(x => settings.Contains(x.Setting))
                   .ToDictionary(x => x.Setting, x => x.Value);
            return settingValues;
        }

        public bool GetBooleanByName(VillageId villageId, VillageSettingEnums setting)
        {
            using var context = _contextFactory.CreateDbContext();
            var settingValue = context.VillagesSetting
                   .Where(x => x.VillageId == villageId.Value)
                   .Where(x => x.Setting == setting)
                   .Select(x => x.Value != 0)
                   .FirstOrDefault();

            return settingValue;
        }

        public void Update(VillageId villageId, Dictionary<VillageSettingEnums, int> settings)
        {
            if (settings.Count == 0) return;
            using var context = _contextFactory.CreateDbContext();

            foreach (var setting in settings)
            {
                context.VillagesSetting
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Setting == setting.Key)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Value, setting.Value));
            }
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