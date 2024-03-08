using MainCore.DTO;
using MainCore.Entities;
using MainCore.UI.Models.Output;

namespace MainCore.Repositories
{
    public interface IQueueBuildingRepository
    {
        void Clean(VillageId villageId);

        int Count(VillageId villageId);

        QueueBuilding GetFirst(VillageId villageId);

        List<ListBoxItem> GetItems(VillageId villageId);

        DateTime GetQueueTime(VillageId villageId);

        bool IsSkippableBuilding(VillageId villageId);

        void Update(VillageId villageId, List<BuildingDto> dtos);

        void Update(VillageId villageId, List<QueueBuildingDto> dtos);
    }
}