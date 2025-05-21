using MainCore.Behaviors;

namespace MainCore.Commands.NextExecute
{
    [Handler]
    [Behaviors(typeof(NextExecuteLoggingBehaviors<,>))]
    public static partial class NextExecuteSleepTaskCommand
    {
        private static async ValueTask HandleAsync(
            SleepTask.Task task,
            ILogger logger,
            ISettingService settingService,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var workTime = settingService.ByName(task.AccountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            task.ExecuteAt = DateTime.Now.AddMinutes(workTime);
        }
    }
}