using MainCore.Tasks.Base;

namespace MainCore.Services
{
    public interface ITaskManager
    {
        void Add<T>(T task, bool first = false) where T : AccountTask;

        void AddOrUpdate<T>(T task, bool first = false) where T : AccountTask;

        void Clear(AccountId accountId);

        BaseTask? GetCurrentTask(AccountId accountId);

        StatusEnums GetStatus(AccountId accountId);

        List<BaseTask> GetTaskList(AccountId accountId);

        TaskQueue GetTaskQueue(AccountId accountId);

        bool IsExecuting(AccountId accountId);

        bool IsExist<T>(AccountId accountId) where T : BaseTask;

        bool IsExist<T>(AccountId accountId, VillageId villageId) where T : BaseTask;

        void Remove<T>(AccountId accountId, VillageId villageId) where T : VillageTask;

        void Remove<T>(AccountId accountId) where T : AccountTask;

        void Remove(AccountId accountId, BaseTask task);

        void ReOrder(AccountId accountId);

        void SetStatus(AccountId accountId, StatusEnums status);

        Task StopCurrentTask(AccountId accountId);
    }
}