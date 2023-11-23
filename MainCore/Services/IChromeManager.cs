using MainCore.Entities;

namespace MainCore.Services
{
    public interface IChromeManager
    {
        IChromeBrowser Get(AccountId accountId);

        void LoadExtension();

        void Shutdown();
    }
}