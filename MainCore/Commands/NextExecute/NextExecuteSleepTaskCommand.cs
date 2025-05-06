namespace MainCore.Commands.NextExecute
{
    [Handler]
    public static partial class NextExecuteSleepTaskCommand
    {
        private static async ValueTask HandleAsync(
            SleepTask.Task task,
            AppDbContext context,
            ITaskManager taskManager,
            CancellationToken cancellationToken)
        {
            
            var workTime = context.ByName(task.AccountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            task.ExecuteAt = DateTime.Now.AddMinutes(workTime);
            await taskManager.ReOrder(task.AccountId);
        }
    }
}