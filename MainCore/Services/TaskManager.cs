using System.Collections.Concurrent;

namespace MainCore.Services;

[RegisterSingleton]
public sealed class TaskManager(RxQueue rxQueue)
{
    private readonly ConcurrentDictionary<AccountId, AccountSchedulerActor> _actors = [];

    public BaseTask? GetCurrentTask(AccountId accountId)
    {
        var tasks = GetTaskList(accountId);
        return tasks.Find(x => x.Stage == StageEnums.Executing);
    }

    public async Task StopCurrentTask(AccountId accountId)
    {
        var actor = GetActor(accountId);
        var cts = actor.CancellationTokenSource;
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

    public void Add<T>(T task, bool first = false) where T : AccountTask
    {
        var existingTask = GetTaskList(task.AccountId).OfType<T>().Any(x => x.Key == task.Key);
        if (existingTask) return;

        var actor = GetActor(task.AccountId);
        actor.Add(task, first);
    }

    public bool IsExist<T>(AccountId accountId) where T : BaseTask
    {
        return GetTaskList(accountId)
            .OfType<T>()
            .Any(x => x.Key == $"{accountId}");
    }

    public bool IsExist<T>(AccountId accountId, VillageId villageId) where T : BaseTask
    {
        return GetTaskList(accountId)
            .OfType<T>()
            .Any(x => x.Key == $"{accountId}-{villageId}");
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

    public void Clear(AccountId accountId)
    {
        var actor = GetActor(accountId);
        actor.Clear();
    }

    public List<BaseTask> GetTaskList(AccountId accountId)
    {
        var actor = GetActor(accountId);
        return actor.Tasks;
    }

    public StatusEnums GetStatus(AccountId accountId)
    {
        var actor = GetActor(accountId);
        return actor.Status;
    }

    public void SetStatus(AccountId accountId, StatusEnums status)
    {
        var actor = GetActor(accountId);
        actor.SetStatus(status);
    }

    public AccountSchedulerActor GetActor(AccountId accountId)
    {
        return _actors.GetOrAdd(accountId, static (id, state) => new AccountSchedulerActor(id, state), rxQueue);
    }
}