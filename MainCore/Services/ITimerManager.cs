
namespace MainCore.Services
{
    public interface ITimerManager
    {
        CancellationTokenSource GetCancellationTokenSource(AccountId accountId);
        StatusEnums GetStatus(AccountId accountId);
        bool IsExecuting(AccountId accountId);
        Task SetStatus(AccountId accountId, StatusEnums status);
        void Shutdown();

        void Start(AccountId accountId);
    }
}