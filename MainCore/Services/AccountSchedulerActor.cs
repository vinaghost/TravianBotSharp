namespace MainCore.Services
{
    public sealed class AccountSchedulerActor(AccountId accountId, RxQueue rxQueue)
    {
        public bool IsExecuting { get; set; } = false;
        public StatusEnums Status { get; set; } = StatusEnums.Offline;
        public CancellationTokenSource? CancellationTokenSource { get; set; }
        public List<BaseTask> Tasks { get; } = [];

        private void AddTask(BaseTask task, bool first)
        {
            if (first)
            {
                var firstTask = Tasks.OrderBy(x => x.ExecuteAt).FirstOrDefault();
                if (firstTask is not null)
                {
                    task.ExecuteAt = firstTask.ExecuteAt.AddHours(-1);
                }
            }

            if (task is VillageTask villageTask)
            {
                rxQueue.Enqueue(new VillageTaskAdded(villageTask));
            }

            Tasks.Add(task);
            rxQueue.Enqueue(new TasksModified(accountId));
        }

        public void Add<T>(T task, bool first) where T : BaseTask
        {
            AddTask(task, first);
        }

        public void Remove(BaseTask task)
        {
            Tasks.Remove(task);
            rxQueue.Enqueue(new TasksModified(accountId));
        }

        public void Remove<T>() where T : AccountTask
        {
            var task = Tasks.OfType<T>().FirstOrDefault(x => x.AccountId == accountId);
            if (task is null) return;

            Tasks.Remove(task);
        }

        public void Remove<T>(VillageId villageId) where T : VillageTask
        {
            var task = Tasks.OfType<T>().FirstOrDefault(x => x.AccountId == accountId && x.VillageId == villageId);
            if (task is null) return;

            Tasks.Remove(task);
        }

        public void Clear()
        {
            if (Tasks.Count == 0) return;

            Tasks.Clear();
            rxQueue.Enqueue(new TasksModified(accountId));
        }

        public void SetStatus(StatusEnums status)
        {
            Status = status;
            rxQueue.Enqueue(new StatusModified(accountId, status));
        }
    }
}