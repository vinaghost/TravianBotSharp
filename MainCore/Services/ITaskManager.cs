using MainCore.Tasks.Base;

namespace MainCore.Services
{
    public interface ITaskManager
    {
        Task Add<T>(T task, bool first = false) where T : AccountTask;

        Task AddOrUpdate<T>(T task, bool first = false) where T : AccountTask;

        Task Clear(AccountId accountId);

        T Get<T>(AccountId accountId) where T : TaskBase;

        T Get<T>(AccountId accountId, VillageId villageId) where T : TaskBase;

        CancellationTokenSource GetCancellationTokenSource(AccountId accountId);

        TaskBase GetCurrentTask(AccountId accountId);

        StatusEnums GetStatus(AccountId accountId);

        List<TaskBase> GetTaskList(AccountId accountId);

        TaskQueue GetTaskQueue(AccountId accountId);

        bool IsExist<T>(AccountId accountId) where T : TaskBase;

        bool IsExist<T>(AccountId accountId, VillageId villageId) where T : TaskBase;

        Task Remove(AccountId accountId, TaskBase task);

        Task ReOrder(AccountId accountId);

        Task SetStatus(AccountId accountId, StatusEnums status);

        Task StopCurrentTast(AccountId accountId);
    }
}