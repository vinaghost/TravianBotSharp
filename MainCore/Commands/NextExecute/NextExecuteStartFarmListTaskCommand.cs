namespace MainCore.Commands.NextExecute
{
    [Handler]
    public static partial class NextExecuteStartFarmListTaskCommand
    {
        private static async ValueTask HandleAsync(
            StartFarmListTask.Task task,
            AppDbContext context,
            ITaskManager taskManager,
            CancellationToken cancellationToken)
        {
            
            var seconds = context.ByName(task.AccountId, AccountSettingEnums.FarmIntervalMin, AccountSettingEnums.FarmIntervalMax);
            task.ExecuteAt = DateTime.Now.AddSeconds(seconds);
            await taskManager.ReOrder(task.AccountId);
        }
    }
}