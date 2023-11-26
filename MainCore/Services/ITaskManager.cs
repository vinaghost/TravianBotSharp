using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Tasks.Base;

namespace MainCore.Services
{
    public interface ITaskManager
    {
        Task Add<T>(AccountId accountId, bool first = false, DateTime executeTime = default) where T : AccountTask;

        Task Add<T>(AccountId accountId, VillageId villageId, bool first = false, DateTime executeTime = default) where T : VillageTask;

        Task AddOrUpdate<T>(AccountId accountId, bool first = false, DateTime executeTime = default) where T : AccountTask;

        Task AddOrUpdate<T>(AccountId accountId, VillageId villageId, bool first = false, DateTime executeTime = default) where T : VillageTask;

        Task Clear(AccountId accountId);

        AccountTask Get<T>(AccountId accountId) where T : AccountTask;

        VillageTask Get<T>(AccountId accountId, VillageId villageId) where T : VillageTask;

        CancellationTokenSource GetCancellationTokenSource(AccountId accountId);

        TaskBase GetCurrentTask(AccountId accountId);

        StatusEnums GetStatus(AccountId accountId);

        TaskManager.TaskInfo GetTaskInfo(AccountId accountId);

        List<TaskBase> GetTaskList(AccountId accountId);

        bool IsExecuting(AccountId accountId);

        bool IsExist<T>(AccountId accountId) where T : AccountTask;

        bool IsExist<T>(AccountId accountId, VillageId villageId) where T : VillageTask;

        Task Remove(AccountId accountId, TaskBase task);

        Task ReOrder(AccountId accountId);

        void SetStatus(AccountId accountId, StatusEnums status);

        Task StopCurrentTask(AccountId accountId);
    }
}