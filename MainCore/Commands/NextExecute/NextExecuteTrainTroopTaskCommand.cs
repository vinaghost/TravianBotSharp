using MainCore.Behaviors;

namespace MainCore.Commands.NextExecute
{
    [Handler]
    [Behaviors(typeof(NextExecuteLoggingBehaviors<,>))]
    public static partial class NextExecuteTrainTroopTaskCommand
    {
        private static async ValueTask HandleAsync(
            TrainTroopTask.Task task,
            TimeSpan queueTime,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            if (queueTime <= TimeSpan.Zero) queueTime = TimeSpan.FromMinutes(1);
            task.ExecuteAt = DateTime.Now.Add(queueTime);
        }
    }
}