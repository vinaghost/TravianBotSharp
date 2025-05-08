using MainCore.Tasks.Base;

namespace MainCore.Services
{
    public interface ITaskManager
    {
        Task Add<T>(T task, bool first = false) where T : AccountTask;

        Task AddOrUpdate<T>(T task, bool first = false) where T : AccountTask;

        Task Clear(AccountId accountId);

        T Get<T>(AccountId accountId) where T : BaseTask;

        T Get<T>(AccountId accountId, VillageId villageId) where T : BaseTask;

        CancellationTokenSource GetCancellationTokenSource(AccountId accountId);

        BaseTask GetCurrentTask(AccountId accountId);

        StatusEnums GetStatus(AccountId accountId);

        List<BaseTask> GetTaskList(AccountId accountId);

        TaskQueue GetTaskQueue(AccountId accountId);

        bool IsExist<T>(AccountId accountId) where T : BaseTask;

        bool IsExist<T>(AccountId accountId, VillageId villageId) where T : BaseTask;

        Task Remove(AccountId accountId, BaseTask task);

        Task ReOrder(AccountId accountId);

        Task SetStatus(AccountId accountId, StatusEnums status);

        Task StopCurrentTast(AccountId accountId);
    }
}