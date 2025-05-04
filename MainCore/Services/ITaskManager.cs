using MainCore.Tasks.Base;

namespace MainCore.Services
{
    public interface ITaskManager
    {
        Task Add(AccountId accountId, TaskBase task, bool first, DateTime executeTime);

        Task Clear(AccountId accountId);

        AccountTask Get<T>(AccountId accountId) where T : AccountTask;

        VillageTask Get<T>(AccountId accountId, VillageId villageId) where T : VillageTask;

        CancellationTokenSource GetCancellationTokenSource(AccountId accountId);

        TaskBase GetCurrentTask(AccountId accountId);

        StatusEnums GetStatus(AccountId accountId);

        List<TaskBase> GetTaskList(AccountId accountId);

        TaskQueue GetTaskQueue(AccountId accountId);

        bool IsExist<T>(AccountId accountId) where T : AccountTask;

        bool IsExist<T>(AccountId accountId, VillageId villageId) where T : VillageTask;

        Task Remove(AccountId accountId, TaskBase task);

        Task ReOrder(AccountId accountId);

        Task SetStatus(AccountId accountId, StatusEnums status);

        Task StopCurrentTast(AccountId accountId);

        Task Update(AccountId accountId, TaskBase task, bool first);
    }
}