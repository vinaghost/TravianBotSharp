namespace MainCore.Commands.Misc
{
    public class SetVillageSettingCommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public SetVillageSettingCommand(IDbContextFactory<AppDbContext> contextFactory = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        }

        public void Execute(VillageId villageId, Dictionary<VillageSettingEnums, int> settings)
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
    }
}