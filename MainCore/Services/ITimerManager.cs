namespace MainCore.Services
{
    public interface ITimerManager
    {
        void Shutdown();

        void Start(AccountId accountId);
    }
}
