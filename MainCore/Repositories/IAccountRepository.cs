using MainCore.UI.Models.Output;

namespace MainCore.Repositories
{
    public interface IAccountRepository
    {
        bool Add(AccountDto dto);

        void Add(List<AccountDetailDto> dtos);

        void Delete(AccountId accountId);

        List<ListBoxItem> GetItems();

        string GetPassword(AccountId accountId);

        string GetUsername(AccountId accountId);

        void Update(AccountDto dto);
    }
}