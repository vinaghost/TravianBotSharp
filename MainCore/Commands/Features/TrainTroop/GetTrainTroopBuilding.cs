namespace MainCore.Commands.Features.TrainTroop
{
    public class GetTrainTroopBuilding
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public GetTrainTroopBuilding(IDbContextFactory<AppDbContext> contextFactory = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        }

        public List<BuildingEnums> Execute(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var settings = new List<VillageSettingEnums>() {
                VillageSettingEnums.BarrackTroop,
                VillageSettingEnums.StableTroop,
                VillageSettingEnums.WorkshopTroop,
            };

            var filterdSettings = context.VillagesSetting
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => settings.Contains(x.Setting))
                .Where(x => x.Value != 0)
                .Select(x => x.Setting)
                .ToList();

            var buildings = new List<BuildingEnums>();

            if (filterdSettings.Contains(VillageSettingEnums.BarrackTroop))
            {
                buildings.Add(BuildingEnums.Barracks);
            }
            if (filterdSettings.Contains(VillageSettingEnums.StableTroop))
            {
                buildings.Add(BuildingEnums.Stable);
            }
            if (filterdSettings.Contains(VillageSettingEnums.WorkshopTroop))
            {
                buildings.Add(BuildingEnums.Workshop);
            }
            return buildings;
        }
    }
}