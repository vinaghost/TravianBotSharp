namespace MainCore.Commands.NextExecute
{
    [Handler]
    [Behaviors]
    public static partial class NextExecuteUpdateVillageTaskCommand
    {
        private static async ValueTask HandleAsync(
            UpdateVillageTask.Task task,
            ILogger logger,
            ISettingService settingService,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var seconds = settingService.ByName(task.VillageId, VillageSettingEnums.AutoRefreshMin, VillageSettingEnums.AutoRefreshMax, 60);
            task.ExecuteAt = DateTime.Now.AddSeconds(seconds);

            logger.Information("Schedule next run at {Time}", task.ExecuteAt.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}