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

            oldTask.ExecuteAt = task.ExecuteAt;
            UpdateTask(oldTask, first);
        }

        public void Remove(BaseTask task)
        {
            if (!_queue.Tasks.Remove(task)) return;

            ReOrderInternal();
        }

        public void Remove<T>() where T : AccountTask
        {
            var task = _queue.Tasks.OfType<T>().FirstOrDefault(x => x.AccountId == accountId);
            if (task is null) return;

            _queue.Tasks.Remove(task);
            ReOrderInternal();
        }

        public void Remove<T>(VillageId villageId) where T : VillageTask
        {
            var task = _queue.Tasks.OfType<T>().FirstOrDefault(x => x.AccountId == accountId && x.VillageId == villageId);
            if (task is null) return;

            _queue.Tasks.Remove(task);
            ReOrderInternal();
        }

        public void ReOrder()
        {
            ReOrderInternal();
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
                var firstTask = _queue.Tasks.FirstOrDefault();
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

            ReOrderInternal();
        }

        private void UpdateTask(AccountTask task, bool first)
        {
            if (first)
            {
                var firstTask = _queue.Tasks.FirstOrDefault();
                if (firstTask is not null)
                {
                    task.ExecuteAt = firstTask.ExecuteAt.AddHours(-1);
                }
            }

            ReOrderInternal();
        }

        private void ReOrderInternal()
        {
            rxQueue.Enqueue(new TasksModified(accountId));
            if (_queue.Tasks.Count <= 1) return;

            _queue.Tasks.Sort((x, y) => DateTime.Compare(x.ExecuteAt, y.ExecuteAt));
        }
    }
}