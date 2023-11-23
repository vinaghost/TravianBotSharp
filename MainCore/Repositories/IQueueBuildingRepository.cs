using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface IQueueBuildingRepository
    {
        void Clean(VillageId villageId);

        int Count(VillageId villageId);

        QueueBuilding GetFirst(VillageId villageId);

        void Update(VillageId villageId, List<BuildingDto> dtos);

        void Update(VillageId villageId, List<QueueBuildingDto> dtos);
    }
}