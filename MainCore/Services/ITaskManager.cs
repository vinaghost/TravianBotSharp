using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Tasks.Base;
using static MainCore.Services.TaskManager;

namespace MainCore.Services
{
    public interface ITaskManager
    {
        void Add<T>(AccountId accountId, bool first = false, DateTime executeTime = default) where T : AccountTask;
        void Add<T>(AccountId accountId, VillageId villageId, bool first = false, DateTime executeTime = default) where T : VillageTask;
        void AddOrUpdate<T>(AccountId accountId, VillageId villageId, bool first = false, DateTime executeTime = default) where T : VillageTask;

        void AddOrUpdate<T>(AccountId accountId, bool first = false, DateTime executeTime = default) where T : AccountTask;

        void Clear(AccountId accountId);

        AccountTask Get<T>(AccountId accountId) where T : AccountTask;

        VillageTask Get<T>(AccountId accountId, VillageId villageId) where T : VillageTask;

        CancellationTokenSource GetCancellationTokenSource(AccountId accountId);

        TaskBase GetCurrentTask(AccountId accountId);

        StatusEnums GetStatus(AccountId accountId);

        TaskInfo GetTaskInfo(AccountId accountId);

        List<TaskBase> GetTaskList(AccountId accountId);
        bool IsExist<T>(AccountId accountId) where T : AccountTask;
        bool IsExist<T>(AccountId accountId, VillageId villageId) where T : VillageTask;
        void Remove(AccountId accountId, TaskBase task);

        void ReOrder(AccountId accountId);

        void SetStatus(AccountId accountId, StatusEnums status);

        Task StopCurrentTask(AccountId accountId);
    }
}