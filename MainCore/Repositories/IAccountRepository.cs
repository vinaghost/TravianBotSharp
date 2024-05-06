using MainCore.UI.Models.Output;

namespace MainCore.Repositories
{
    public interface IAccountRepository
    {
        void Delete(AccountId accountId);

        List<ListBoxItem> GetItems();
    }
}