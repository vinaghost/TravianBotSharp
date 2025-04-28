using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

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

        public TaskBase GetCurrentTask(AccountId accountId)
        {
            var tasks = GetTaskList(accountId);
            return tasks.Find(x => x.Stage == StageEnums.Executing);
        }

        public async Task StopCurrentTast(AccountId accountId)
        {
            var cts = GetCancellationTokenSource(accountId);
            if (cts is null) return;
            await cts.CancelAsync();
            TaskBase currentTask;
            do
            {
                currentTask = GetCurrentTask(accountId);
                if (currentTask is null) return;
                await Task.Delay(500);
            }
            while (currentTask.Stage != StageEnums.Waiting);
            await SetStatus(accountId, StatusEnums.Paused);
        }

        public async Task AddOrUpdate<T>(AccountId accountId, bool first = false, DateTime executeTime = default) where T : AccountTask
        {
            var task = Get<T>(accountId);
            if (task is null)
            {
                await Add<T>(accountId, first, executeTime);
            }
            else
            {
                await Update(accountId, task, first);
            }
        }

        public async Task AddOrUpdate<T>(AccountId accountId, VillageId villageId, bool first = false, DateTime executeTime = default) where T : VillageTask
        {
            var task = Get<T>(accountId, villageId);
            if (task is null)
            {
                await Add<T>(accountId, villageId, first, executeTime);
            }
            else
            {
                await Update(accountId, task, first);
            }
        }

        public async Task Add<T>(AccountId accountId, bool first = false, DateTime executeTime = default) where T : AccountTask
        {
            var task = Locator.Current.GetService<T>();
            task.Setup(accountId);
            task.ExecuteAt = executeTime;
            await Add(accountId, task, first, executeTime);
        }

        public async Task Add<T>(AccountId accountId, VillageId villageId, bool first = false, DateTime executeTime = default) where T : VillageTask
        {
            var task = Locator.Current.GetService<T>();
            await task.Setup(accountId, villageId);
            task.ExecuteAt = executeTime;
            await Add(accountId, task, first, executeTime);
        }

        public AccountTask Get<T>(AccountId accountId) where T : AccountTask
        {
            var tasks = GetTaskList(accountId);
            var filteredTasks = tasks.OfType<T>();
            var task = filteredTasks.FirstOrDefault(x => x.AccountId == accountId);
            return task;
        }

        public VillageTask Get<T>(AccountId accountId, VillageId villageId) where T : VillageTask
        {
            var tasks = GetTaskList(accountId);
            var filteredTasks = tasks.OfType<T>();
            var task = filteredTasks.FirstOrDefault(x => x.AccountId == accountId && x.VillageId == villageId);
            return task;
        }

        public bool IsExist<T>(AccountId accountId) where T : AccountTask
        {
            var tasks = GetTaskList(accountId);
            var filteredTasks = tasks.OfType<T>();
            return filteredTasks.Any(x => x.AccountId == accountId);
        }

        public bool IsExist<T>(AccountId accountId, VillageId villageId) where T : VillageTask
        {
            var tasks = GetTaskList(accountId);
            var filteredTasks = tasks.OfType<T>();
            return filteredTasks.Any(x => x.AccountId == accountId && x.VillageId == villageId);
        }

        private async Task Add(AccountId accountId, TaskBase task, bool first, DateTime executeTime)
        {
            var tasks = GetTaskList(accountId);

            if (first)
            {
                var firstTask = tasks.FirstOrDefault();
                if (firstTask is not null)
                {
                    if (firstTask.ExecuteAt > DateTime.Now)
                    {
                        task.ExecuteAt = DateTime.Now;
                    }
                    else
                    {
                        task.ExecuteAt = firstTask.ExecuteAt.AddHours(-1);
                    }
                }
                else
                {
                    task.ExecuteAt = DateTime.Now;
                }
            }
            else
            {
                if (executeTime == default)
                {
                    task.ExecuteAt = DateTime.Now;
                }
                else
                {
                    task.ExecuteAt = executeTime;
                }
            }

            tasks.Add(task);
            await ReOrder(accountId, tasks);
        }

        private async Task Update(AccountId accountId, TaskBase task, bool first)
        {
            var tasks = GetTaskList(accountId);

            if (first)
            {
                var firstTask = tasks.FirstOrDefault();
                if (firstTask is not null)
                {
                    if (firstTask.ExecuteAt > DateTime.Now)
                    {
                        task.ExecuteAt = DateTime.Now;
                    }
                    else
                    {
                        task.ExecuteAt = firstTask.ExecuteAt.AddHours(-1);
                    }
                }
            }
            else
            {
                task.ExecuteAt = DateTime.Now;
            }

            await ReOrder(accountId, tasks);
        }

        public async Task Remove(AccountId accountId, TaskBase task)
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

        private async Task ReOrder(AccountId accountId, List<TaskBase> tasks)
        {
            if (tasks.Count <= 1) return;
            tasks.Sort((x, y) => DateTime.Compare(x.ExecuteAt, y.ExecuteAt));
            await _taskUpdated.HandleAsync(new(accountId));
        }

        public List<TaskBase> GetTaskList(AccountId accountId)
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
        public List<TaskBase> Tasks = [];
    }
}