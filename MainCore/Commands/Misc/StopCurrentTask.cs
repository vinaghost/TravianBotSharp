using MainCore.Notification;
using MainCore.Tasks.Base;

namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class StopCurrentTask
    {
        public sealed record Command(AccountId AccountId) : ByAccountIdBase(AccountId);

        private static async ValueTask HandleAsync(
            Command command,
            ITaskManager taskManager, ITimerManager timerManager,
            CancellationToken cancellationToken)
        {
            var accountId = command.AccountId;
            var cts = timerManager.GetCancellationTokenSource(accountId);
            if (cts is null) return;
            await cts.CancelAsync();
            TaskBase currentTask;
            do
            {
                currentTask = taskManager.GetCurrentTask(accountId);
                if (currentTask is null) return;
                await Task.Delay(500);
            }
            while (currentTask.Stage != StageEnums.Waiting);
            await timerManager.SetStatus(accountId, StatusEnums.Paused);
        }
    }
}