using MainCore.Tasks.Base;

namespace MainCore.Services
{
    [RegisterSingleton<ITaskManager, TaskManager>]
    public sealed record TaskManager : ITaskManager
    {
        private readonly Dictionary<AccountId, TaskQueue> _queues = new();

        public event Action<AccountId> StatusUpdated = delegate { };

        public event Action<AccountId> TaskUpdated = delegate { };

        public BaseTask? GetCurrentTask(AccountId accountId)
        {
            var tasks = GetTaskList(accountId);
            return tasks.Find(x => x.Stage == StageEnums.Executing);
        }

        public async Task StopCurrentTask(AccountId accountId)
        {
            var cts = GetCancellationTokenSource(accountId);
            if (cts is not null) await cts.CancelAsync();

            BaseTask? currentTask;
            do
            {
                currentTask = GetCurrentTask(accountId);
                if (currentTask is null) break;
                await Task.Delay(500);
            }
            while (currentTask.Stage != StageEnums.Waiting);
            SetStatus(accountId, StatusEnums.Paused);
        }

        public void AddOrUpdate<T>(T task, bool first = false) where T : AccountTask
        {
            var oldTask = Get<T>(task.AccountId, task.Key);
            if (oldTask is null)
            {
                Add<T>(task, first);
            }
            else
            {
                Update(oldTask, first);
            }
        }

        public void Add<T>(T task, bool first = false) where T : AccountTask
        {
            AddTask(task, first);
        }

        private T? Get<T>(AccountId accountId, string key) where T : BaseTask
        {
            var task = GetTaskList(accountId)
                .OfType<T>()
                .FirstOrDefault(x => x.Key == key);
            return task;
        }

        public bool IsExist<T>(AccountId accountId) where T : BaseTask
        {
            var tasks = GetTaskList(accountId)
                .OfType<T>();
            return tasks.Any(x => x.Key == $"{accountId}");
        }

        public bool IsExist<T>(AccountId accountId, VillageId villageId) where T : BaseTask
        {
            var tasks = GetTaskList(accountId)
                .OfType<T>();
            return tasks.Any(x => x.Key == $"{accountId}-{villageId}");
        }

        private void AddTask(AccountTask task, bool first)
        {
            var tasks = GetTaskList(task.AccountId);

            if (first)
            {
                var firstTask = tasks.FirstOrDefault();
                if (firstTask is not null && firstTask.ExecuteAt < task.ExecuteAt)
                {
                    task.ExecuteAt = firstTask.ExecuteAt.AddHours(-1);
                }
            }

            tasks.Add(task);
            ReOrder(task.AccountId, tasks);
        }

        private void Update(AccountTask task, bool first)
        {
            var tasks = GetTaskList(task.AccountId);

            if (first)
            {
                var firstTask = tasks.FirstOrDefault();
                if (firstTask is not null && firstTask.ExecuteAt < task.ExecuteAt)
                {
                    task.ExecuteAt = firstTask.ExecuteAt.AddHours(-1);
                }
                else
                {
                    task.ExecuteAt = DateTime.Now;
                }
            }
            else
            {
                task.ExecuteAt = DateTime.Now;
            }

            ReOrder(task.AccountId, tasks);
        }

        public void Remove(AccountId accountId, BaseTask task)
        {
            var tasks = GetTaskList(accountId);
            if (tasks.Remove(task))
            {
                ReOrder(accountId, tasks);
            }
        }

        public void Remove<T>(AccountId accountId) where T : AccountTask
        {
            var tasks = GetTaskList(accountId);
            var task = tasks.OfType<T>().FirstOrDefault(x => x.AccountId == accountId);
            if (task is null) return;
            tasks.Remove(task);
            ReOrder(accountId, tasks);
        }

        public void Remove<T>(AccountId accountId, VillageId villageId) where T : VillageTask
        {
            var tasks = GetTaskList(accountId);
            var task = tasks.OfType<T>().FirstOrDefault(x => x.AccountId == accountId && x.VillageId == villageId);
            if (task is null) return;
            tasks.Remove(task);
            ReOrder(accountId, tasks);
        }

        public void ReOrder(AccountId accountId)
        {
            var tasks = GetTaskList(accountId);
            ReOrder(accountId, tasks);
        }

        public void Clear(AccountId accountId)
        {
            var tasks = GetTaskList(accountId);
            if (tasks.Count == 0) return;
            tasks.Clear();
            TaskUpdated.Invoke(accountId);
        }

        private void ReOrder(AccountId accountId, List<BaseTask> tasks)
        {
            if (tasks.Count <= 1) return;
            tasks.Sort((x, y) => DateTime.Compare(x.ExecuteAt, y.ExecuteAt));
            TaskUpdated.Invoke(accountId);
        }

        public List<BaseTask> GetTaskList(AccountId accountId)
        {
            var queue = GetTaskQueue(accountId);
            return queue.Tasks;
        }

        public StatusEnums GetStatus(AccountId accountId)
        {
            var queue = GetTaskQueue(accountId);
            return queue.Status;
        }

        public void SetStatus(AccountId accountId, StatusEnums status)
        {
            var queue = GetTaskQueue(accountId);
            queue.Status = status;
            StatusUpdated.Invoke(accountId);
        }

        private CancellationTokenSource? GetCancellationTokenSource(AccountId accountId)
        {
            var queue = GetTaskQueue(accountId);
            return queue.CancellationTokenSource;
        }

        public bool IsExecuting(AccountId accountId)
        {
            var queue = GetTaskQueue(accountId);
            return queue.IsExecuting;
        }

        public TaskQueue GetTaskQueue(AccountId accountId)
        {
            if (_queues.ContainsKey(accountId))
            {
                return _queues[accountId];
            }
            else
            {
                var queue = new TaskQueue();
                _queues.Add(accountId, queue);
                return queue;
            }
        }
    }

    public class TaskQueue
    {
        public bool IsExecuting { get; set; } = false;
        public StatusEnums Status { get; set; } = StatusEnums.Offline;
        public CancellationTokenSource? CancellationTokenSource { get; set; }
        public List<BaseTask> Tasks { get; } = [];
    }
}