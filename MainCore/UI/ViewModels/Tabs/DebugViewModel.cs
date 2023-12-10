using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Services;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class DebugViewModel : AccountTabViewModelBase
    {
        private readonly LogSink _logSink;
        private readonly ITaskManager _taskManager;
        private readonly MessageTemplateTextFormatter _formatter;
        public ObservableCollection<TaskItem> Tasks { get; } = new();

        private string _cacheLog;
        public string _logs;

        public string Logs
        {
            get => _logs;
            set => this.RaiseAndSetIfChanged(ref _logs, value);
        }

        private bool _isLogLoading = false;
        private const string _discordUrl = "https://ko-fi.com/vinaghost";

        public DebugViewModel(ILogEventSink logSink, ITaskManager taskManager)
        {
            _logSink = logSink as LogSink;
            _logSink.LogEmitted += LogEmitted;
            _taskManager = taskManager;

            _formatter = new("{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

            LoadTask = ReactiveCommand.Create<AccountId, List<TaskItem>>(LoadTaskHandler);
            LoadLog = ReactiveCommand.Create<AccountId>(LoadLogHandler);
            GetHelpCommand = ReactiveCommand.Create(GetHelpTask);
            LogFolderCommand = ReactiveCommand.Create(LogFolderTask);

            LoadTask.Subscribe(items =>
            {
                Tasks.Clear();
                items.ForEach(Tasks.Add);
            });
            LoadLog.Subscribe(logs =>
            {
                Logs = _cacheLog;
            });
        }

        private void LogEmitted(AccountId accountId, LogEvent logEvent)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            if (_isLogLoading) return;

            var buffer = new StringWriter(new StringBuilder());
            _formatter.Format(logEvent, buffer);
            buffer.WriteLine(_cacheLog);
            _cacheLog = buffer.ToString();

            RxApp.MainThreadScheduler.Schedule(() => Logs = _cacheLog);
        }

        public async Task TaskListRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadTask.Execute(accountId).SubscribeOn(RxApp.TaskpoolScheduler);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadTask.Execute(accountId).SubscribeOn(RxApp.TaskpoolScheduler);
            await LoadLog.Execute(accountId).SubscribeOn(RxApp.TaskpoolScheduler);
        }

        private List<TaskItem> LoadTaskHandler(AccountId accountId)
        {
            var tasks = _taskManager.GetTaskList(accountId);

            return tasks
                .Select(x => new TaskItem(x))
                .ToList();
        }

        private void LoadLogHandler(AccountId accountId)
        {
            _isLogLoading = true;
            var logs = _logSink.GetLogs(accountId);
            var buffer = new StringWriter(new StringBuilder());

            foreach (var log in logs)
            {
                _formatter.Format(log, buffer);
            }
            _cacheLog = buffer.ToString();
            _isLogLoading = false;
        }

        private void GetHelpTask()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _discordUrl,
                UseShellExecute = true
            });
        }

        private void LogFolderTask()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(AppContext.BaseDirectory, "logs"),
                UseShellExecute = true
            });
        }

        public ReactiveCommand<AccountId, List<TaskItem>> LoadTask { get; }
        public ReactiveCommand<AccountId, Unit> LoadLog { get; }
        public ReactiveCommand<Unit, Unit> GetHelpCommand { get; }
        public ReactiveCommand<Unit, Unit> LogFolderCommand { get; }
    }
}