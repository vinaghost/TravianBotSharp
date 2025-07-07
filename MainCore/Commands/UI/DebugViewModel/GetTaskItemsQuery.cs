using MainCore.UI.Models.Output;

namespace MainCore.Commands.UI.DebugViewModel
{
    [Handler]
    public static partial class GetTaskItemsQuery
    {
        public sealed record Query(AccountId AccountId) : IAccountQuery;

        private static async ValueTask<List<TaskItem>> HandleAsync(
            Query query,
            ITaskManager taskManager
        )
        {
            await Task.CompletedTask;
            var tasks = taskManager
                .GetTaskList(query.AccountId)
                .Select(x => new TaskItem(x))
                .ToList();
            return tasks;
        }
    }
}