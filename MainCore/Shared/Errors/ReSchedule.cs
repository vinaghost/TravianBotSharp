namespace MainCore.Shared.Errors;

public sealed class ReSchedule : Error
{
    private ReSchedule(DateTime executeAt) : base($"Reschedule this task at {executeAt:yyyy-MM-dd HH:mm:ss}")
    {
        ExecuteAt = executeAt;
    }

    public DateTime ExecuteAt { get; }

    public static Result Error(DateTime executeAt) => new ReSchedule(executeAt);
}