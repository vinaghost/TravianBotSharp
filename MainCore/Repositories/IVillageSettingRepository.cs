namespace MainCore.Repositories
{
    public interface IVillageSettingRepository
    {
        Dictionary<VillageSettingEnums, int> Get(VillageId villageId);
    }
}