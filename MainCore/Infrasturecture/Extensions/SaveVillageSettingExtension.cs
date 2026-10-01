namespace MainCore.Infrasturecture.Extensions
{
    public static class SaveVillageSettingExtension
    {
        public static void SaveVillageSetting(this AppDbContext context, VillageId villageId, Dictionary<VillageSettingEnums, int> settings)
        {
            if (settings.Count == 0) return;
            foreach (var setting in settings)
            {
                context.VillagesSetting
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Setting == setting.Key)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Value, setting.Value));
            }
        }
    }
}