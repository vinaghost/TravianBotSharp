using MainCore.DTO;
using MainCore.Entities;
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
        void Update(AccountId accountId, List<VillageDto> dtos);
        VillageId GetVillageHasRallypoint(AccountId accountId);
    }
}