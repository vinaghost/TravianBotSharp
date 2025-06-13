using System.Text;

namespace MainCore.Services
{
    // This service is registered manually in AppMixins
    public sealed class TelegramCommandService
    {
        private readonly ITelegramService _telegramService;
        private readonly ITaskManager _taskManager;

        public TelegramCommandService(ITelegramService telegramService, ITaskManager taskManager)
        {
            _telegramService = telegramService;
            _taskManager = taskManager;
            _telegramService.CommandReceived += OnCommandReceived;
        }

        private async void OnCommandReceived(AccountId accountId, string message)
        {
            if (!string.Equals(message.Trim(), "tasks", StringComparison.OrdinalIgnoreCase)) return;

            var tasks = _taskManager.GetTaskList(accountId);
            if (tasks.Count == 0)
            {
                await _telegramService.SendText("No tasks in queue", accountId);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Task\tExecute at\tStage");
            foreach (var task in tasks)
            {
                sb.AppendLine($"{task.Description}\t{task.ExecuteAt:yyyy-MM-dd HH:mm:ss}\t{task.Stage}");
            }

            await _telegramService.SendText(sb.ToString(), accountId);
        }
    }
}
