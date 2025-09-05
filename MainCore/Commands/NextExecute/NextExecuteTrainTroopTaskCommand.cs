namespace MainCore.Commands.NextExecute
{
    [Handler]
    public static partial class NextExecuteTrainTroopTaskCommand
    {
        public sealed record Command(TrainTroopTask.Task Task) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            ISettingService settingService
            )
        {
            await Task.CompletedTask;
            var seconds = settingService.ByName(
                command.Task.VillageId,
                VillageSettingEnums.TrainTroopRepeatTimeMin,
                VillageSettingEnums.TrainTroopRepeatTimeMax,
                60
            );

            command.Task.ExecuteAt = DateTime.Now.AddSeconds(seconds);
        }
    }
}
