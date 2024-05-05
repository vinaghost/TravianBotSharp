namespace MainCore.Repositories
{
    public interface IStorageRepository
    {
        Result IsEnoughResource(VillageId villageId, long[] requiredResource);

        long[] GetMissingResource(VillageId villageId, long[] requiredResource);

        int GetGranaryPercent(VillageId villageId);
    }
}