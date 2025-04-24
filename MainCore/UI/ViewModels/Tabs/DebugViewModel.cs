using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Serilog.Core;
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
        private readonly ITimerManager _timerManager;
        private static readonly ExpressionTemplate _template = new("{@t:HH:mm:ss} [{@l:u3}] {@m}\n{@x}");
        private const string NotOpen = "Chrome didn't open yet";

        public ObservableCollection<TaskItem> Tasks { get; } = [];

        [Reactive]
        private string _logs;

        [Reactive]
        private string _endpointAddress;

        public DebugViewModel(ILogEventSink logSink, ITaskManager taskManager, ITimerManager timerManager)
        {
            _taskManager = taskManager;
            _logSink = logSink as LogSink;
            _logSink.LogEmitted += LogEmitted;

            LoadTaskCommand.Subscribe(items =>
            {
                Tasks.Clear();
                items.ForEach(Tasks.Add);
            });

            LoadLogCommand.BindTo(this, vm => vm.Logs);
            LoadEndpointAddressCommand.BindTo(this, vm => vm.EndpointAddress);
            _timerManager = timerManager;
        }

        private void LogEmitted(AccountId accountId, LogEvent logEvent)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            LoadLogCommand.Execute(accountId).Subscribe();
        }

        public async Task EndpointAddressRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadEndpointAddressCommand.Execute(accountId);
        }

        public async Task TaskListRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadTaskCommand.Execute(accountId);
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
            var tasks = _taskManager.GetTaskList(accountId);

            return tasks
                .Select(x => new TaskItem(x))
                .ToList();
        }

        [ReactiveCommand]
        private string LoadLog(AccountId accountId)
        {
            var logs = _logSink.GetLogs(accountId);
            var buffer = new StringWriter(new StringBuilder());

            foreach (var log in logs)
            {
                _template.Format(log, buffer);
            }
            return buffer.ToString();
        }

        [ReactiveCommand]
        private string LoadEndpointAddress(AccountId accountId)
        {
            var status = _timerManager.GetStatus(accountId);
            if (status == StatusEnums.Offline) return NotOpen;
            var address = "address enpoint is disabled";
            if (string.IsNullOrEmpty(address)) return NotOpen;
            return address;
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