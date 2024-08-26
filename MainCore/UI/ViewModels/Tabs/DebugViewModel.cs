using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton]
    public class DebugViewModel : AccountTabViewModelBase
    {
        private readonly LogSink _logSink;
        private readonly ITaskManager _taskManager;
        private readonly IChromeManager _chromeManager;
        private static readonly MessageTemplateTextFormatter _formatter = new("{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
        private const string NotOpen = "Chrome didn't open yet";

        public ObservableCollection<TaskItem> Tasks { get; } = new();

        private string _logs;

        public string Logs
        {
            get => _logs;
            set => this.RaiseAndSetIfChanged(ref _logs, value);
        }

        private string _endpointAddress;

        public string EndpointAddress
        {
            get => _endpointAddress;
            set => this.RaiseAndSetIfChanged(ref _endpointAddress, value);
        }

        public DebugViewModel(ILogEventSink logSink, ITaskManager taskManager, IChromeManager chromeManager)
        {
            _taskManager = taskManager;
            _chromeManager = chromeManager;
            _logSink = logSink as LogSink;
            _logSink.LogEmitted += LogEmitted;

            LoadTask = ReactiveCommand.Create<AccountId, List<TaskItem>>(LoadTaskHandler);
            LoadLog = ReactiveCommand.Create<AccountId, string>(LoadLogHandler);
            LoadEndpointAddress = ReactiveCommand.Create<AccountId, string>(LoadEndpointAddressHandler);
            LeftCommand = ReactiveCommand.Create(LeftTask);
            RightCommand = ReactiveCommand.Create(RightTask);

            LoadTask.Subscribe(items =>
            {
                Tasks.Clear();
                items.ForEach(Tasks.Add);
            });
            LoadLog.Subscribe(logs =>
            {
                Logs = logs;
            });
            LoadEndpointAddress.Subscribe(address =>
            {
                EndpointAddress = address;
            });
        }

        private void LogEmitted(AccountId accountId, LogEvent logEvent)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            LoadLog.Execute(accountId).Subscribe();
        }

        public async Task EndpointAddressRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadEndpointAddress.Execute(accountId);
        }

        public async Task TaskListRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadTask.Execute(accountId);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadTask.Execute(accountId);
            await LoadLog.Execute(accountId);
            await LoadEndpointAddress.Execute(accountId);
        }

        private List<TaskItem> LoadTaskHandler(AccountId accountId)
        {
            var tasks = _taskManager.GetTaskList(accountId);

            return tasks
                .Select(x => new TaskItem(x))
                .ToList();
        }

        private string LoadLogHandler(AccountId accountId)
        {
            var logs = _logSink.GetLogs(accountId);
            var buffer = new StringWriter(new StringBuilder());

            foreach (var log in logs)
            {
                _formatter.Format(log, buffer);
            }
            return buffer.ToString();
        }

        private string LoadEndpointAddressHandler(AccountId accountId)
        {
            var status = _taskManager.GetStatus(accountId);
            if (status == StatusEnums.Offline) return NotOpen;
            var chromeBrowser = _chromeManager.Get(accountId);
            var address = chromeBrowser.EndpointAddress;
            if (string.IsNullOrEmpty(address)) return NotOpen;
            return address;
        }

        private void LeftTask()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://ko-fi.com/vinaghost",
                UseShellExecute = true
            });
        }

        private void RightTask()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(AppContext.BaseDirectory, "logs"),
                UseShellExecute = true
            });
        }

        public ReactiveCommand<AccountId, List<TaskItem>> LoadTask { get; }
        public ReactiveCommand<AccountId, string> LoadLog { get; }
        public ReactiveCommand<AccountId, string> LoadEndpointAddress { get; }
        public ReactiveCommand<Unit, Unit> LeftCommand { get; }
        public ReactiveCommand<Unit, Unit> RightCommand { get; }
    }
}