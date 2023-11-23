using MainCore.DTO;
using MainCore.Entities;
using MainCore.UI.Models.Output;

namespace MainCore.Repositories
{
    public interface IFarmRepository
    {
        List<FarmId> GetActive(AccountId accountId);
        void ChangeActive(FarmId farmListId);
        int CountActive(AccountId accountId);
        List<ListBoxItem> GetItems(AccountId accountId);
        void Update(AccountId accountId, List<FarmDto> dtos);
    }
}