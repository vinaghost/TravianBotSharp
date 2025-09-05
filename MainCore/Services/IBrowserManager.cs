namespace MainCore.Services
{
    public interface IBrowserManager
    {
        IBrowser Get(AccountId accountId);

        void LoadExtension();

        Task Shutdown();
    }
}
