using MainCore.Constraints;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class GetNormalBuildingsQuery
    {
        public sealed record Query(VillageId VillageId, BuildingId BuildingId) : IVillageQuery;

        private static async ValueTask<List<BuildingEnums>> HandleAsync(
            Query query,
            GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery,
            CancellationToken cancellationToken
            )
        {
            var (villageId, buildingId) = query;

            var buildingItems = await getLayoutBuildingsQuery.HandleAsync(new(query.VillageId), cancellationToken);

            var type = buildingItems
                .Where(x => x.Id == buildingId)
                .Select(x => x.Type)
                .FirstOrDefault();

            if (type != BuildingEnums.Site) return [type];

            var buildings = buildingItems
                .Select(x => x.Type)
                .Where(x => !MultipleBuildings.Contains(x))
                .Distinct()
                .ToList();

            return AvailableBuildings.Where(x => !buildings.Contains(x)).ToList();
        }

        private static readonly List<BuildingEnums> MultipleBuildings =
        [
            BuildingEnums.Warehouse,
            BuildingEnums.Granary,
            BuildingEnums.Trapper,
            BuildingEnums.Cranny,
        ];

        private static readonly List<BuildingEnums> IgnoreBuildings =
        [
            BuildingEnums.Site,
            BuildingEnums.Blacksmith,
            BuildingEnums.CityWall,
            BuildingEnums.EarthWall,
            BuildingEnums.Palisade,
            BuildingEnums.WW,
            BuildingEnums.StoneWall,
            BuildingEnums.MakeshiftWall,
            BuildingEnums.Unknown,
        ];

        private static readonly IEnumerable<BuildingEnums> AvailableBuildings = Enum.GetValues(typeof(BuildingEnums))
            .Cast<BuildingEnums>()
            .Where(x => !IgnoreBuildings.Contains(x));
    }
}