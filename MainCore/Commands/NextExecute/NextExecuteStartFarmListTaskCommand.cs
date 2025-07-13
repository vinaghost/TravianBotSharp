namespace MainCore.Commands.NextExecute
{
    [Handler]
    public static partial class NextExecuteStartFarmListTaskCommand
    {
        public sealed record Command(StartFarmListTask.Task Task) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            ISettingService settingService
            )
        {
            await Task.CompletedTask;
            var seconds = settingService.ByName(
                command.Task.AccountId,
                AccountSettingEnums.FarmIntervalMin,
                AccountSettingEnums.FarmIntervalMax);
            command.Task.ExecuteAt = DateTime.Now.AddSeconds(seconds);
        }
    }
}