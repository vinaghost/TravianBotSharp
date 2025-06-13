using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Services
{
    // This service is registered manually in AppMixins
    public sealed class TelegramCommandService
    {
        private readonly ITelegramService _telegramService;
        private readonly ITaskManager _taskManager;
        private readonly ICustomServiceScopeFactory _serviceScopeFactory;

        public TelegramCommandService(ITelegramService telegramService, ITaskManager taskManager, ICustomServiceScopeFactory serviceScopeFactory)
        {
            _telegramService = telegramService;
            _taskManager = taskManager;
            _serviceScopeFactory = serviceScopeFactory;
            _telegramService.CommandReceived += OnCommandReceived;
        }

        private async void OnCommandReceived(AccountId accountId, string message)
        {
            var command = message.Trim();
            switch (command.ToLowerInvariant())
            {
                case "tasks":
                    await SendTasks(accountId);
                    break;
                case "pause":
                    await Pause(accountId);
                    break;
                case "resume":
                    Resume(accountId);
                    break;
                case "restart":
                    await Restart(accountId);
                    break;
                default:
                    break;
            }
        }

        private async Task SendTasks(AccountId accountId)
        {
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

        private async Task Pause(AccountId accountId)
        {
            if (_taskManager.GetStatus(accountId) == StatusEnums.Online)
            {
                await _taskManager.StopCurrentTask(accountId);
            }
        }

        private void Resume(AccountId accountId)
        {
            if (_taskManager.GetStatus(accountId) == StatusEnums.Paused)
            {
                _taskManager.SetStatus(accountId, StatusEnums.Online);
            }
        }

        private async Task Restart(AccountId accountId)
        {
            if (_taskManager.GetStatus(accountId) != StatusEnums.Paused) return;

            _taskManager.SetStatus(accountId, StatusEnums.Starting);
            _taskManager.Clear(accountId);

            using (var scope = _serviceScopeFactory.CreateScope(accountId))
            {
                await scope.ServiceProvider.GetRequiredService<AccountInit.Handler>().HandleAsync(new(accountId));
            }

            _taskManager.SetStatus(accountId, StatusEnums.Online);
        }
    }
}
