using MainCore.Commands.Features.TrainTroop;
using MainCore.Constraints;

namespace MainCore.Queries
{
    [Handler]
    public static partial class GetTrainTroopBuildingQuery
    {
        public sealed record Query(VillageId VillageId) : IVillageQuery;

        private static async ValueTask<List<BuildingEnums>> HandleAsync(
            Query query,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var villageId = query.VillageId;

            var settings = context.VillagesSetting
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => TrainTroopCommand.BuildingSettings.Values.Contains(x.Setting))
                .Where(x => x.Value != 0)
                .Select(x => x.Setting)
                .ToList();

            var buildings = new List<BuildingEnums>();

            if (settings.Contains(VillageSettingEnums.BarrackTroop))
            {
                buildings.Add(BuildingEnums.Barracks);
            }
            if (settings.Contains(VillageSettingEnums.StableTroop))
            {
                buildings.Add(BuildingEnums.Stable);
            }
            if (settings.Contains(VillageSettingEnums.GreatBarrackTroop))
            {
                buildings.Add(BuildingEnums.GreatBarracks);
            }
            if (settings.Contains(VillageSettingEnums.GreatStableTroop))
            {
                buildings.Add(BuildingEnums.GreatStable);
            }
            if (settings.Contains(VillageSettingEnums.WorkshopTroop))
            {
                buildings.Add(BuildingEnums.Workshop);
            }

            return buildings;
        }
    }
}