namespace MainCore.Commands.NextExecute
{
    [Handler]
    public static partial class NextExecuteStartFarmListTaskCommand
    {
        private static async ValueTask HandleAsync(
            StartFarmListTask.Task task,
            ISettingService settingService,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var seconds = settingService.ByName(task.AccountId, AccountSettingEnums.FarmIntervalMin, AccountSettingEnums.FarmIntervalMax);
            task.ExecuteAt = DateTime.Now.AddSeconds(seconds);
        }
    }
}