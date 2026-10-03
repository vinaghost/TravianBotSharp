using MainCore.Tasks.Base;
using System.Threading.Channels;

namespace MainCore.Services.Scheduling
{
    internal sealed class AccountSchedulerActor(AccountId accountId, IRxQueue rxQueue)
    {
        private readonly TaskQueue _queue = new();

        public TaskQueue GetQueue() => _queue;

        public List<BaseTask> GetTaskList() => _queue.Tasks;

        public CancellationTokenSource? GetCancellationTokenSource() => _queue.CancellationTokenSource;

        public bool IsExecuting() => _queue.IsExecuting;

        public StatusEnums GetStatus() => _queue.Status;

        public void Add(AccountTask task, bool first)
        {
            AddTask(task, first);
        }

        public void AddOrUpdate(AccountTask task, bool first)
        {
            var oldTask = _queue.Tasks
                .OfType<AccountTask>()
                    .FirstOrDefault(x => x.Key == task.Key);

            if (oldTask is null)
            {
                AddTask(task, first);
                return;
            }

            if (first)
            {
                var firstTask = _queue.Tasks.OrderBy(x => x.ExecuteAt).FirstOrDefault();
                if (firstTask is not null)
                {
                    oldTask.ExecuteAt = firstTask.ExecuteAt.AddHours(-1);
                }
                else
                {
                    oldTask.ExecuteAt = task.ExecuteAt;
                }
            }
            else
            {
                oldTask.ExecuteAt = task.ExecuteAt;
            }
        }

        public void Remove(BaseTask task)
        {
            _queue.Tasks.Remove(task);
        }

        public void Remove<T>() where T : AccountTask
        {
            var task = _queue.Tasks.OfType<T>().FirstOrDefault(x => x.AccountId == accountId);
            if (task is null) return;

            _queue.Tasks.Remove(task);
        }

        public void Remove<T>(VillageId villageId) where T : VillageTask
        {
            var task = _queue.Tasks.OfType<T>().FirstOrDefault(x => x.AccountId == accountId && x.VillageId == villageId);
            if (task is null) return;

            _queue.Tasks.Remove(task);
        }

        public void Clear()
        {
            if (_queue.Tasks.Count == 0)
            {
                return;
            }

            _queue.Tasks.Clear();
            rxQueue.Enqueue(new TasksModified(accountId));
        }

        public void SetStatus(StatusEnums status)
        {
            _queue.Status = status;
            rxQueue.Enqueue(new StatusModified(accountId, status));
        }

        private void AddTask(AccountTask task, bool first)
        {
            if (first)
            {
                var firstTask = _queue.Tasks.OrderBy(x => x.ExecuteAt).FirstOrDefault();
                if (firstTask is not null)
                {
                    task.ExecuteAt = firstTask.ExecuteAt.AddHours(-1);
                }
            }

            _queue.Tasks.Add(task);
            if (task is VillageTask villageTask)
            {
                rxQueue.Enqueue(new VillageTaskAdded(villageTask));
            }
        }
    }
}