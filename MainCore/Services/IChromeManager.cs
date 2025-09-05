namespace MainCore.Services
{
    public interface IChromeManager
    {
        IBrowser Get(AccountId accountId);

        void LoadExtension();

        Task Shutdown();
    }
}
