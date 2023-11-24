using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Tasks.Base;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Splat;

namespace MainCore.Services
{
    [RegisterAsSingleton]
    public sealed class TaskManager : ITaskManager
    {
        public class TaskInfo
        {
            public bool IsExecuting { get; set; } = false;
            public StatusEnums Status { get; set; } = StatusEnums.Offline;
            public CancellationTokenSource CancellationTokenSource { get; set; } = null;

            public List<TaskBase> TaskList { get; set; } = new();
        }

        private readonly Dictionary<AccountId, TaskInfo> _tasks = new();
        private readonly IMediator _mediator;

        public TaskManager(IMediator mediator)
        {
            _mediator = mediator;
        }

        public TaskInfo GetTaskInfo(AccountId accountId)
        {
            var task = _tasks.GetValueOrDefault(accountId);
            if (task is null)
            {
                task = new();
                _tasks.Add(accountId, task);
            }
            return task;
        }

        public List<TaskBase> GetTaskList(AccountId accountId)
        {
            var taskInfo = GetTaskInfo(accountId);
            return taskInfo.TaskList;
        }

        public StatusEnums GetStatus(AccountId accountId)
        {
            var taskInfo = GetTaskInfo(accountId);
            return taskInfo.Status;
        }

        public void SetStatus(AccountId accountId, StatusEnums status)
        {
            var taskInfo = GetTaskInfo(accountId);
            taskInfo.Status = status;
            _mediator.Publish(new StatusUpdated(accountId, status));
        }

        public bool IsExecuting(AccountId accountId)
        {
            var taskInfo = GetTaskInfo(accountId);
            return taskInfo.IsExecuting;
        }

        public CancellationTokenSource GetCancellationTokenSource(AccountId accountId)
        {
            var taskInfo = GetTaskInfo(accountId);
            return taskInfo.CancellationTokenSource;
        }

        public TaskBase GetCurrentTask(AccountId accountId)
        {
            var tasks = GetTaskList(accountId);
            return tasks.FirstOrDefault(x => x.Stage == StageEnums.Executing);
        }

        public async Task StopCurrentTask(AccountId accountId)
        {
            var cts = GetCancellationTokenSource(accountId);
            cts?.Cancel();
            TaskBase currentTask;
            do
            {
                currentTask = GetCurrentTask(accountId);
                if (currentTask is null) return;
                await Task.Delay(500);
            }
            while (currentTask.Stage != StageEnums.Waiting);
            SetStatus(accountId, StatusEnums.Offline);
        }

        public void AddOrUpdate<T>(AccountId accountId, bool first = false, DateTime executeTime = default) where T : AccountTask
        {
            var task = Get<T>(accountId);
            if (task is null)
            {
                Add<T>(accountId, first, executeTime);
            }
            else
            {
                Update(accountId, task, first, executeTime);
            }
        }

        public void AddOrUpdate<T>(AccountId accountId, VillageId villageId, bool first = false, DateTime executeTime = default) where T : VillageTask
        {
            var task = Get<T>(accountId, villageId);
            if (task is null)
            {
                Add<T>(accountId, villageId, first, executeTime);
            }
            else
            {
                Update(accountId, task, first, executeTime);
            }
        }

        public void Add<T>(AccountId accountId, bool first = false, DateTime executeTime = default) where T : AccountTask
        {
            var task = Locator.Current.GetService<T>();
            task.Setup(accountId);
            task.ExecuteAt = executeTime;
            Add(accountId, task, first);
        }

        public void Add<T>(AccountId accountId, VillageId villageId, bool first = false, DateTime executeTime = default) where T : VillageTask
        {
            var task = Locator.Current.GetService<T>();
            task.Setup(accountId, villageId);
            task.ExecuteAt = executeTime;
            Add(accountId, task, first);
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

        private void Add(AccountId accountId, TaskBase task, bool first = false)
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

            if (task.ExecuteAt == default) task.ExecuteAt = DateTime.Now;

            tasks.Add(task);
            ReOrder(accountId, tasks);
        }

        private void Update(AccountId accountId, TaskBase task, bool first = false, DateTime executeTime = default)
        {
            var tasks = GetTaskList(accountId);

            if (first)
            {
                var firstTask = tasks.FirstOrDefault();
                if (firstTask is not null)
                {
                    task.ExecuteAt = firstTask.ExecuteAt.AddSeconds(-1);
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
            ReOrder(accountId, tasks);
        }

        public void ReOrder(AccountId accountId)
        {
            var tasks = GetTaskList(accountId);
            ReOrder(accountId, tasks);
        }

        public void Remove(AccountId accountId, TaskBase task)
        {
            var tasks = GetTaskList(accountId);
            tasks.Remove(task);
            ReOrder(accountId, tasks);
        }

        public void Clear(AccountId accountId)
        {
            var tasks = GetTaskList(accountId);
            tasks.Clear();
            _mediator.Publish(new TaskUpdated(accountId));
        }

        private void ReOrder(AccountId accountId, List<TaskBase> tasks)
        {
            tasks.Sort((x, y) => DateTime.Compare(x.ExecuteAt, y.ExecuteAt));
            _mediator.Publish(new TaskUpdated(accountId));
        }
    }
}