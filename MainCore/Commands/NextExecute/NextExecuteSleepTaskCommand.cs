namespace MainCore.Commands.NextExecute
{
    [Handler]
    public static partial class NextExecuteSleepTaskCommand
    {
        public sealed record Command(SleepTask.Task Task) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            ISettingService settingService
            )
        {
            await Task.CompletedTask;
            var workTime = settingService.ByName(
                command.Task.AccountId,
                AccountSettingEnums.WorkTimeMin,
                AccountSettingEnums.WorkTimeMax,
                60);
            command.Task.ExecuteAt = DateTime.Now.AddSeconds(workTime);
        }
    }
}
