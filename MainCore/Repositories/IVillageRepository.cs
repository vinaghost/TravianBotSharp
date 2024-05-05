using MainCore.UI.Models.Output;

namespace MainCore.Repositories
{
    public interface IVillageRepository
    {
        List<VillageId> Get(AccountId accountId);

        VillageId GetActiveVillages(AccountId accountId);

        List<VillageId> GetHasBuildingJobVillages(AccountId accountId);

        List<VillageId> GetInactiveVillages(AccountId accountId);

        List<VillageId> GetMissingBuildingVillages(AccountId accountId);

        List<ListBoxItem> GetItems(AccountId accountId);

        string GetVillageName(VillageId villageId);
    }
}