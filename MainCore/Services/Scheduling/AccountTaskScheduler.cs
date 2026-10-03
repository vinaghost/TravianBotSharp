using MainCore.Tasks.Base;
using System.Collections.Concurrent;

namespace MainCore.Services.Scheduling
{
    [RegisterSingleton<IAccountTaskScheduler, AccountTaskScheduler>]
    public sealed class AccountTaskScheduler(
        IRxQueue rxQueue,
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger logger) : IAccountTaskScheduler, IDisposable
    {
        private readonly ConcurrentDictionary<AccountId, AccountSchedulerActor> _actors = [];

        public void Add(AccountTask task, bool first = false)
        {
            if (!ValidateTaskOwnership(task))
            {
                return;
            }

            var actor = GetActor(task.AccountId);
            actor.Add(task, first);
        }

        public void AddOrUpdate(AccountTask task, bool first = false)
        {
            if (!ValidateTaskOwnership(task))
            {
                return;
            }

            var actor = GetActor(task.AccountId);
            actor.AddOrUpdate(task, first);
        }

        public void Remove(AccountId accountId, BaseTask task)
        {
            var actor = GetActor(accountId);
            actor.Remove(task);
        }

        public void Remove<T>(AccountId accountId) where T : AccountTask
        {
            var actor = GetActor(accountId);
            actor.Remove<T>();
        }

        public void Remove<T>(AccountId accountId, VillageId villageId) where T : VillageTask
        {
            var actor = GetActor(accountId);
            actor.Remove<T>(villageId);
        }

        public void ReOrder(AccountId accountId)
        {
            var actor = GetActor(accountId);
            actor.ReOrder();
        }

        public void Clear(AccountId accountId)
        {
            var actor = GetActor(accountId);
            actor.Clear();
        }

        public StatusEnums GetStatus(AccountId accountId)
        {
            var actor = GetActor(accountId);
            return actor.GetStatus();
        }

        public void SetStatus(AccountId accountId, StatusEnums status)
        {
            var actor = GetActor(accountId);
            actor.SetStatus(status);
        }

        public bool IsExecuting(AccountId accountId)
        {
            var actor = GetActor(accountId);
            return actor.IsExecuting();
        }

        public List<BaseTask> GetTaskList(AccountId accountId)
        {
            var actor = GetActor(accountId);
            return actor.GetTaskList();
        }

        public TaskQueue GetQueue(AccountId accountId)
        {
            var actor = GetActor(accountId);
            return actor.GetQueue();
        }

        public CancellationTokenSource? GetCancellationTokenSource(AccountId accountId)
        {
            var actor = GetActor(accountId);
            return actor.GetCancellationTokenSource();
        }

        private AccountSchedulerActor GetActor(AccountId accountId)
        {
            return _actors.GetOrAdd(accountId, static (id, state) => new AccountSchedulerActor(id, state), rxQueue);
        }

        private bool ValidateTaskOwnership(AccountTask task)
        {
            if (task.AccountId == AccountId.Empty)
            {
                var reason = $"Rejected {task.GetType().Name}: AccountId is empty";
                logger.Warning("{Reason}", reason);
                return false;
            }

            if (task is VillageTask villageTask)
            {
                using var context = contextFactory.CreateDbContext();
                var villageBelongToAccount = context.Villages
                    .Any(x => x.Id == villageTask.VillageId.Value && x.AccountId == task.AccountId.Value);

                if (!villageBelongToAccount)
                {
                    var reason = $"Rejected {task.GetType().Name}: village {villageTask.VillageId.Value} does not belong to account {task.AccountId.Value}";
                    logger.Warning("{Reason}", reason);
                    return false;
                }
            }
            return true;
        }

        public void Dispose()
        {
            _actors.Clear();
        }
    }
}