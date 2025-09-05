namespace MainCore.Commands.NextExecute
{
    [Handler]
    public static partial class NextExecuteUpdateVillageTaskCommand
    {
        public sealed record Command(UpdateVillageTask.Task Task) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            ISettingService settingService
            )
        {
            await Task.CompletedTask;
            var seconds = settingService.ByName(
                command.Task.VillageId,
                VillageSettingEnums.AutoRefreshMin,
                VillageSettingEnums.AutoRefreshMax,
                60);
            command.Task.ExecuteAt = DateTime.Now.AddSeconds(seconds);
        }
    }
}
