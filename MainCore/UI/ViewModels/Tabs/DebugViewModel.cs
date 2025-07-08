using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using Serilog.Events;
using Serilog.Templates;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<DebugViewModel>]
    public partial class DebugViewModel : AccountTabViewModelBase
    {
        private readonly LogSink _logSink;
        private readonly ITaskManager _taskManager;
        private static readonly ExpressionTemplate _template = new("{@t:HH:mm:ss} [{@l:u3}] {@m}\n{@x}");

        public ObservableCollection<TaskItem> Tasks { get; } = [];

        [Reactive]
        private string _logs = "";

        [Reactive]
        private string _endpointAddress = "";

        public DebugViewModel(LogSink logSink, ITaskManager taskManager)
        {
            _logSink = logSink;
            logSink.LogEmitted += LogEmitted;
            _taskManager = taskManager;
            taskManager.TaskUpdated += TaskListRefresh;

            LoadTaskCommand.Subscribe(items =>
            {
                Tasks.Clear();
                items.ForEach(Tasks.Add);
            });

            LoadLogCommand.BindTo(this, vm => vm.Logs);
            LoadEndpointAddressCommand.BindTo(this, vm => vm.EndpointAddress);
        }

        private void LogEmitted(AccountId accountId, LogEvent logEvent)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            LoadLogCommand.Execute(accountId).Subscribe();
        }

        private void TaskListRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            LoadTaskCommand.Execute(accountId).Subscribe();
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadTaskCommand.Execute(accountId);
            await LoadLogCommand.Execute(accountId);
            await LoadEndpointAddressCommand.Execute(accountId);
        }

        [ReactiveCommand]
        private List<TaskItem> LoadTask(AccountId accountId)
        {
            var tasks = _taskManager
               .GetTaskList(accountId)
               .Select(x => new TaskItem(x))
               .ToList();
            return tasks;
        }

        [ReactiveCommand]
        private string LoadLog(AccountId accountId)
        {
            var logs = _logSink.GetLogs(accountId);
            using var sw = new StringWriter(new StringBuilder());

            foreach (var log in logs)
            {
                _template.Format(log, sw);
            }
            return sw.ToString();
        }

        [ReactiveCommand]
        private string LoadEndpointAddress(AccountId accountId)
        {
            return "Address endpoint is disabled";
        }

        [ReactiveCommand]
        private void Left()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://ko-fi.com/vinaghost",
                UseShellExecute = true
            });
        }

        [ReactiveCommand]
        private void Right()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(AppContext.BaseDirectory, "logs"),
                UseShellExecute = true
            });
        }
    }
}