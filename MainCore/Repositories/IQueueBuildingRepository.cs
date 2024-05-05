using MainCore.UI.Models.Output;

namespace MainCore.Repositories
{
    public interface IQueueBuildingRepository
    {
        void Clean(VillageId villageId);

        int Count(VillageId villageId);

        QueueBuilding GetFirst(VillageId villageId);

        List<ListBoxItem> GetItems(VillageId villageId);

        bool IsSkippableBuilding(VillageId villageId);
    }
}