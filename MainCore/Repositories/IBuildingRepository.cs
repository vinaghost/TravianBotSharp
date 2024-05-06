using MainCore.UI.Models.Output;

namespace MainCore.Repositories
{
    public interface IBuildingRepository
    {
        List<ListBoxItem> GetItems(VillageId villageId);

        List<BuildingEnums> GetNormalBuilding(VillageId villageId, BuildingId buildingId);

        bool IsRallyPointExists(VillageId villageId);

        void UpdateWall(VillageId villageId);
    }
}