using MainCore.UI.Models.Output;

namespace MainCore.Repositories
{
    public interface IFarmRepository
    {
        void ChangeActive(FarmId farmListId);

        int CountActive(AccountId accountId);

        List<ListBoxItem> GetItems(AccountId accountId);
    }
}