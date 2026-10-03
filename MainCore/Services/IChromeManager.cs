namespace MainCore.Services
{
    public interface IChromeManager
    {
        IChromeBrowser Get(AccountId accountId);

        Task Shutdown();
    }
}