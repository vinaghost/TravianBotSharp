namespace MainCore.Commands.NextExecute
{
    [Handler]
    public static partial class NextExecuteTrainTroopTaskCommand
    {
        private static async ValueTask HandleAsync(
            TrainTroopTask.Task task,
            ISettingService settingService,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var seconds = settingService.ByName(
                task.VillageId,
                VillageSettingEnums.TrainTroopRepeatTimeMin,
                VillageSettingEnums.TrainTroopRepeatTimeMax
            );

            task.ExecuteAt = DateTime.Now.AddSeconds(seconds);
        }
    }
}