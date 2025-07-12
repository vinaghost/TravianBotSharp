using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
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

        public DebugViewModel(LogSink logSink, ITaskManager taskManager, IRxQueue rxQueue)
        {
            _logSink = logSink;
            _taskManager = taskManager;

            LoadTaskCommand.Subscribe(items =>
            {
                Tasks.Clear();
                items.ForEach(Tasks.Add);
            });

            LoadLogCommand.BindTo(this, vm => vm.Logs);
            LoadEndpointAddressCommand.BindTo(this, vm => vm.EndpointAddress);

            rxQueue.RegisterCommand<LogEmitted>(LogEmittedCommand);
            rxQueue.RegisterCommand<TasksModified>(TasksModifiedCommand);
        }

        [ReactiveCommand]
        private async Task LogEmitted(LogEmitted notification)
        {
            if (!IsActive) return;
            var (accountId, logEvent) = notification;
            if (accountId != AccountId) return;

            using var sw = new StringWriter(new StringBuilder());
            _template.Format(logEvent, sw);
            sw.Write(Logs);

            await Observable.Start(() =>
            {
                Logs = sw.ToString();
            }, RxApp.MainThreadScheduler);
        }

        [ReactiveCommand]
        private async Task TasksModified(TasksModified notification)
        {
            if (!IsActive) return;
            var accountId = notification.AccountId;
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