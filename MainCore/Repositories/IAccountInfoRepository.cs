using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface IAccountInfoRepository
    {
        string GetDiscordWebhookUrl(AccountId accountId);

        int GetFreeSlotCP(AccountId accountId);

        string GetTemplatePath(AccountId accountId);

        bool IsPlusActive(AccountId accountId);

        void SetDiscordWebhookUrl(AccountId accountId, string url);

        void SetTemplatePath(AccountId accountId, string path);

        void Update(AccountId accountId, AccountInfoDto dto);
    }
}