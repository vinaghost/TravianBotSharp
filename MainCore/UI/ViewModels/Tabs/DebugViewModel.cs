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

        public DebugViewModel(ILogEventSink logSink, ITaskManager taskManager)
        {
            _logSink = logSink as LogSink;
            _logSink.LogEmitted += LogEmitted;
            _taskManager = taskManager;

            _formatter = new("{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
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

            Observable.Start(
                () => Logs = _cacheLog,
                RxApp.MainThreadScheduler);
        }

        public async Task TaskListRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadTask(accountId);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadTask(accountId);
            await LoadLog(accountId);
        }

        private async Task LoadTask(AccountId accountId)
        {
            Tasks.Clear();
            var tasks = _taskManager.GetTaskList(accountId);
            if (tasks.Count == 0) return;

            await Observable.Start(() =>
            {
                tasks
                .Select(x => new TaskItem(x))
                .ToList()
                .ForEach(Tasks.Add);
            }, RxApp.MainThreadScheduler);
        }

        private async Task LoadLog(AccountId accountId)
        {
            _isLogLoading = true;
            var logs = _logSink.GetLogs(accountId);
            var buffer = new StringWriter(new StringBuilder());
            foreach (var log in logs)
            {
                _formatter.Format(log, buffer);
            }
            _cacheLog = buffer.ToString();

            await Observable.Start(() =>
            {
                Logs = _cacheLog;
            }, RxApp.MainThreadScheduler);
            _isLogLoading = false;
        }
    }
}