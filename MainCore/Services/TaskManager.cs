using MainCore.Tasks.Base;

namespace MainCore.Services
{
    [RegisterSingleton<ITaskManager, TaskManager>]
    public sealed class TaskManager : ITaskManager
    {
        private readonly Dictionary<AccountId, TaskQueue> _queues = new();
        private readonly TaskUpdated.Handler _taskUpdated;
        private readonly StatusUpdated.Handler _statusUpdated;

        public TaskManager(TaskUpdated.Handler taskUpdated, StatusUpdated.Handler statusUpdated)
        {
            _taskUpdated = taskUpdated;
            _statusUpdated = statusUpdated;
        }

        public BaseTask GetCurrentTask(AccountId accountId)
        {
            var tasks = GetTaskList(accountId);
            return tasks.Find(x => x.Stage == StageEnums.Executing);
        }

        public async Task StopCurrentTast(AccountId accountId)
        {
            var cts = GetCancellationTokenSource(accountId);
            if (cts is null) return;
            await cts.CancelAsync();
            BaseTask currentTask;
            do
            {
                currentTask = GetCurrentTask(accountId);
                if (currentTask is null) return;
                await Task.Delay(500);
            }
            while (currentTask.Stage != StageEnums.Waiting);
            await SetStatus(accountId, StatusEnums.Paused);
        }

        public async Task AddOrUpdate<T>(T task, bool first = false) where T : AccountTask
        {
            var oldTask = Get<T>(task.AccountId, task.Key);
            if (oldTask is null)
            {
                await Add<T>(task, first);
            }
            else
            {
                await Update(oldTask, first);
            }
        }

        public async Task Add<T>(T task, bool first = false) where T : AccountTask
        {
            await Add(task, first);
        }

        public T Get<T>(AccountId accountId) where T : BaseTask
        {
            var task = GetTaskList(accountId)
                .OfType<T>()
                .FirstOrDefault(x => x.Key == $"{accountId}");
            return task;
        }

        public T Get<T>(AccountId accountId, VillageId villageId) where T : BaseTask
        {
            var task = GetTaskList(accountId)
                .OfType<T>()
                .FirstOrDefault(x => x.Key == $"{accountId}-{villageId}");
            return task;
        }

        private T Get<T>(AccountId accountId, string key) where T : BaseTask
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

        private async Task Add(AccountTask task, bool first)
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
            await ReOrder(task.AccountId, tasks);
        }

        private async Task Update(AccountTask task, bool first)
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

            await ReOrder(task.AccountId, tasks);
        }

        public async Task Remove(AccountId accountId, BaseTask task)
        {
            var tasks = GetTaskList(accountId);
            if (tasks.Remove(task))
            {
                await ReOrder(accountId, tasks);
            }
        }

        public async Task ReOrder(AccountId accountId)
        {
            var tasks = GetTaskList(accountId);
            await ReOrder(accountId, tasks);
        }

        public async Task Clear(AccountId accountId)
        {
            var tasks = GetTaskList(accountId);
            if (tasks.Count == 0) return;
            tasks.Clear();
            await _taskUpdated.HandleAsync(new(accountId));
        }

        private async Task ReOrder(AccountId accountId, List<BaseTask> tasks)
        {
            if (tasks.Count <= 1) return;
            tasks.Sort((x, y) => DateTime.Compare(x.ExecuteAt, y.ExecuteAt));
            await _taskUpdated.HandleAsync(new(accountId));
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

        public async Task SetStatus(AccountId accountId, StatusEnums status)
        {
            var queue = GetTaskQueue(accountId);
            queue.Status = status;
            await _statusUpdated.HandleAsync(new(accountId));
        }

        public CancellationTokenSource GetCancellationTokenSource(AccountId accountId)
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
        public CancellationTokenSource CancellationTokenSource { get; set; } = null;
        public List<BaseTask> Tasks = [];
    }
}