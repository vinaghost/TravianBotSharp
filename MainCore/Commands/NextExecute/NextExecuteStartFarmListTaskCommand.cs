namespace MainCore.Commands.NextExecute
{
    [Handler]
    [Behaviors]
    public static partial class NextExecuteStartFarmListTaskCommand
    {
        private static async ValueTask HandleAsync(
            StartFarmListTask.Task task,
            ISettingService settingService,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var seconds = settingService.ByName(task.AccountId, AccountSettingEnums.FarmIntervalMin, AccountSettingEnums.FarmIntervalMax);
            task.ExecuteAt = DateTime.Now.AddSeconds(seconds);

            logger.Information("Schedule next run at {Time}", task.ExecuteAt.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}