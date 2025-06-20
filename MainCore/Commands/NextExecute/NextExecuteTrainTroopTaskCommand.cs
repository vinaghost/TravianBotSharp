using MainCore.Behaviors;

namespace MainCore.Commands.NextExecute
    {
        [Handler]
        [Behaviors(typeof(NextExecuteLoggingBehaviors<,>))]
        public static partial class NextExecuteTrainTroopTaskCommand
        {
            public sealed record Command(TrainTroopTask.Task Task, TimeSpan QueueTime);

            private static async ValueTask HandleAsync(
                Command command,
                ILogger logger,
                CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                var (task, queueTime) = command;
                if (queueTime <= TimeSpan.Zero) queueTime = TimeSpan.FromMinutes(1);
                task.ExecuteAt = DateTime.Now.Add(queueTime);
            }
        }
    }
