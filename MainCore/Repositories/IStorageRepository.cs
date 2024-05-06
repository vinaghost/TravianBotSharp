namespace MainCore.Repositories
{
    public interface IStorageRepository
    {
        int GetGranaryPercent(VillageId villageId);
    }
}