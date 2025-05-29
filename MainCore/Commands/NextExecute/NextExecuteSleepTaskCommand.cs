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
            var workTime = settingService.ByName(
                task.AccountId,
                AccountSettingEnums.WorkTimeMin,
                AccountSettingEnums.WorkTimeMax,
                60);
            task.ExecuteAt = DateTime.Now.AddSeconds(workTime);
        }
    }
}