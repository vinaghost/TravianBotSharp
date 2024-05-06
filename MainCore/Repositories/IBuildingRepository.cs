using MainCore.Common.Models;
using MainCore.UI.Models.Output;

namespace MainCore.Repositories
{
    public interface IBuildingRepository
    {
        int CountQueueBuilding(VillageId villageId);

        int CountResourceQueueBuilding(VillageId villageId);

        List<ListBoxItem> GetItems(VillageId villageId);

        NormalBuildPlan GetNormalBuildPlan(VillageId villageId, ResourceBuildPlan plan);

        bool IsRallyPointExists(VillageId villageId);

        List<BuildingItem> GetBuildings(VillageId villageId, bool ignoreJobBuilding = false);

        List<BuildingEnums> GetNormalBuilding(VillageId villageId, BuildingId buildingId);

        void UpdateWall(VillageId villageId);
    }
}