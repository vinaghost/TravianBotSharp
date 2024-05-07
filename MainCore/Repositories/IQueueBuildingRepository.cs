namespace MainCore.Repositories
{
    public interface IQueueBuildingRepository
    {
        void Clean(VillageId villageId);

        int Count(VillageId villageId);

        bool IsSkippableBuilding(VillageId villageId);
    }
}