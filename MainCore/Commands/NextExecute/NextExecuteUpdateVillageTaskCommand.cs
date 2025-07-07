namespace MainCore.Commands.NextExecute
{
    [Handler]
    public static partial class NextExecuteUpdateVillageTaskCommand
    {
        private static async ValueTask HandleAsync(
            UpdateVillageTask.Task task,
            ISettingService settingService
            )
        {
            await Task.CompletedTask;
            var seconds = settingService.ByName(
                task.VillageId,
                VillageSettingEnums.AutoRefreshMin,
                VillageSettingEnums.AutoRefreshMax,
                60);
            task.ExecuteAt = DateTime.Now.AddSeconds(seconds);
        }
    }
}