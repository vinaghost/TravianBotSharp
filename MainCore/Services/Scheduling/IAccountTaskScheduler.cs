using MainCore.Tasks.Base;

namespace MainCore.Services.Scheduling
{
    public interface IAccountTaskScheduler
    {
        void Add(AccountTask task, bool first = false);

        void AddOrUpdate(AccountTask task, bool first = false);

        void Clear(AccountId accountId);

        CancellationTokenSource? GetCancellationTokenSource(AccountId accountId);

        TaskQueue GetQueue(AccountId accountId);

        StatusEnums GetStatus(AccountId accountId);

        List<BaseTask> GetTaskList(AccountId accountId);

        bool IsExecuting(AccountId accountId);

        void Remove(AccountId accountId, BaseTask task);

        void Remove<T>(AccountId accountId) where T : AccountTask;

        void Remove<T>(AccountId accountId, VillageId villageId) where T : VillageTask;

        void SetStatus(AccountId accountId, StatusEnums status);
    }
}