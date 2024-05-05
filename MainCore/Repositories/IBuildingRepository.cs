using MainCore.Common.Models;
using MainCore.UI.Models.Output;

namespace MainCore.Repositories
{
    public interface IBuildingRepository
    {
        int CountQueueBuilding(VillageId villageId);

        int CountResourceQueueBuilding(VillageId villageId);

        BuildingDto GetBuilding(VillageId villageId, int location);

        List<ListBoxItem> GetItems(VillageId villageId);

        Building GetCropland(VillageId villageId);

        NormalBuildPlan GetNormalBuildPlan(VillageId villageId, ResourceBuildPlan plan);

        bool EmptySite(VillageId villageId, int location);

        bool IsRallyPointExists(VillageId villageId);

        List<BuildingItem> GetBuildings(VillageId villageId, bool ignoreJobBuilding = false);

        List<BuildingEnums> GetTrainTroopBuilding(VillageId villageId);

        int GetBuildingLocation(VillageId villageId, BuildingEnums building);

        List<BuildingEnums> GetNormalBuilding(VillageId villageId, BuildingId buildingId);

        void UpdateWall(VillageId villageId);
    }
}