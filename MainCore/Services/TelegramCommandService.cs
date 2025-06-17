using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using MainCore.Commands.UI.Villages.BuildViewModel;

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

        private static string[] Tokenize(string command)
        {
            var tokens = new List<string>();
            var sb = new StringBuilder();
            var inQuote = false;

            foreach (var ch in command)
            {
                if (ch == '"')
                {
                    inQuote = !inQuote;
                    continue;
                }

                if (char.IsWhiteSpace(ch) && !inQuote)
                {
                    if (sb.Length > 0)
                    {
                        tokens.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }

            if (sb.Length > 0)
                tokens.Add(sb.ToString());

            return tokens.ToArray();
        }

        private async void OnCommandReceived(AccountId accountId, string message)
        {
            var command = message.Trim();

            // build related commands have parameters, handle them separately
            if (command.StartsWith("build", StringComparison.OrdinalIgnoreCase))
            {
                var tokens = Tokenize(command);
                await HandleBuildCommand(accountId, tokens);
                return;
            }

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

        private async Task HandleBuildCommand(AccountId accountId, string[] tokens)
        {
            if (tokens.Length < 3) return;

            var villageName = tokens[2];
            VillageId? villageId;
            using (var scope = _serviceScopeFactory.CreateScope(accountId))
            {
                var getVillageIdQuery = scope.ServiceProvider.GetRequiredService<GetVillageIdByNameQuery.Handler>();
                villageId = await getVillageIdQuery.HandleAsync(new(accountId, villageName));
            }
            if (villageId is null)
            {
                await _telegramService.SendText($"Village '{villageName}' not found", accountId);
                return;
            }

            switch (tokens[1])
            {
                case "list":
                    await SendBuildList(accountId, villageId.Value);
                    break;
                case "pause":
                    BuildPause(accountId, villageId.Value);
                    break;
                case "resume":
                    await BuildResume(accountId, villageId.Value);
                    break;
                case "rem":
                    if (tokens.Length < 4) return;
                    if (int.TryParse(tokens[3], out var index))
                        await RemoveBuildJob(accountId, villageId.Value, index);
                    break;
                case "add":
                    if (tokens.Length < 5) return;
                    if (!Enum.TryParse<BuildingEnums>(tokens[3], true, out var building)) return;
                    if (int.TryParse(tokens[4], out var level))
                        await AddBuildJob(accountId, villageId.Value, building, level);
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
                // omit year from timestamp to keep the output concise
                sb.AppendLine($"{task.Description}\t{task.ExecuteAt:MM-dd HH:mm:ss}\t{task.Stage}");
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

        private async Task SendBuildList(AccountId accountId, VillageId villageId)
        {
            using var scope = _serviceScopeFactory.CreateScope(accountId);
            var getJobItemsQuery = scope.ServiceProvider.GetRequiredService<GetJobItemsQuery.Handler>();
            var jobs = await getJobItemsQuery.HandleAsync(new(villageId));
            if (jobs.Count == 0)
            {
                await _telegramService.SendText("No build jobs", accountId);
                return;
            }

            var sb = new StringBuilder();
            for (var i = 0; i < jobs.Count; i++)
            {
                sb.AppendLine($"{i + 1}. {jobs[i].Content}");
            }

            await _telegramService.SendText(sb.ToString(), accountId);
        }

        private void BuildPause(AccountId accountId, VillageId villageId)
        {
            _taskManager.Remove<UpgradeBuildingTask.Task>(accountId, villageId);
        }

        private async Task BuildResume(AccountId accountId, VillageId villageId)
        {
            using var scope = _serviceScopeFactory.CreateScope(accountId);
            var getVillageNameQuery = scope.ServiceProvider.GetRequiredService<GetVillageNameQuery.Handler>();
            var name = await getVillageNameQuery.HandleAsync(new(villageId));
            _taskManager.AddOrUpdate<UpgradeBuildingTask.Task>(new(accountId, villageId, name));
        }

        private async Task RemoveBuildJob(AccountId accountId, VillageId villageId, int index)
        {
            using var scope = _serviceScopeFactory.CreateScope(accountId);
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var jobId = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .OrderBy(x => x.Position)
                .Skip(index - 1)
                .Select(x => x.Id)
                .FirstOrDefault();
            if (jobId == 0) return;

            var deleteJobByIdCommand = scope.ServiceProvider.GetRequiredService<DeleteJobByIdCommand.Handler>();
            await deleteJobByIdCommand.HandleAsync(new(villageId, new JobId(jobId)));
            var jobUpdated = scope.ServiceProvider.GetRequiredService<JobUpdated.Handler>();
            await jobUpdated.HandleAsync(new(accountId, villageId));
        }

        private async Task AddBuildJob(AccountId accountId, VillageId villageId, BuildingEnums building, int level)
        {
            using var scope = _serviceScopeFactory.CreateScope(accountId);
            var normalBuildCommand = scope.ServiceProvider.GetRequiredService<NormalBuildCommand.Handler>();
            var plan = new NormalBuildPlan { Location = 0, Type = building, Level = level };
            var result = await normalBuildCommand.HandleAsync(new(villageId, plan));
            if (result.IsFailed)
            {
                await _telegramService.SendText(result.ToString(), accountId);
                return;
            }

            var jobUpdated = scope.ServiceProvider.GetRequiredService<JobUpdated.Handler>();
            await jobUpdated.HandleAsync(new(accountId, villageId));
        }
    }
}
