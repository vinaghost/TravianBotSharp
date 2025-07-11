namespace MainCore.Commands.NextExecute
{
    [Handler]
    public static partial class NextExecuteSleepTaskCommand
    {
        private static async ValueTask HandleAsync(
            SleepTask.Task task,
            ISettingService settingService
            )
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