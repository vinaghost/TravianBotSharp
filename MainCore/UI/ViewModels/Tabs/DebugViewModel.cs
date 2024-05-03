using MainCore.Commands.UI.Debug;
using MainCore.Commands.UI.DebugTab;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using Serilog.Core;
using Serilog.Events;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class DebugViewModel : AccountTabViewModelBase
    {
        private readonly LogSink _logSink;
        private readonly IMediator _mediator;
        public ObservableCollection<TaskItem> Tasks { get; } = new();

        public string _logs;

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

        public DebugViewModel(ILogEventSink logSink, IMediator mediator)
        {
            _logSink = logSink as LogSink;
            _logSink.LogEmitted += LogEmitted;
            _mediator = mediator;

            LoadTask = ReactiveCommand.CreateFromTask<AccountId, List<TaskItem>>(LoadTaskHandler);
            LoadLog = ReactiveCommand.CreateFromTask<AccountId, string>(LoadLogHandler);
            LoadEndpointAddress = ReactiveCommand.CreateFromTask<AccountId, string>(LoadEndpointAddressHandler);
            LeftCommand = ReactiveCommand.CreateFromTask(LeftTask);
            RightCommand = ReactiveCommand.CreateFromTask(RightTask);

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
            LoadLog.Execute(accountId).SubscribeOn(RxApp.TaskpoolScheduler).Subscribe();
        }

        public async Task EndpointAddressRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadEndpointAddress.Execute(accountId).SubscribeOn(RxApp.TaskpoolScheduler);
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
            await LoadEndpointAddress.Execute(accountId).SubscribeOn(RxApp.TaskpoolScheduler);
        }

        private async Task<List<TaskItem>> LoadTaskHandler(AccountId accountId)
        {
            return await _mediator.Send(new GetTaskCommand(accountId));
        }

        private async Task<string> LoadLogHandler(AccountId accountId)
        {
            var logs = await _mediator.Send(new GetLogCommand(AccountId));
            return logs;
        }

        private async Task<string> LoadEndpointAddressHandler(AccountId accountId)
        {
            return await _mediator.Send(new LoadEndpointAddress(accountId));
        }

        private async Task LeftTask()
        {
            await _mediator.Send(new LeftCommand());
        }

        private async Task RightTask()
        {
            await _mediator.Send(new RightCommand());
        }

        public ReactiveCommand<AccountId, List<TaskItem>> LoadTask { get; }
        public ReactiveCommand<AccountId, string> LoadLog { get; }
        public ReactiveCommand<AccountId, string> LoadEndpointAddress { get; }
        public ReactiveCommand<Unit, Unit> LeftCommand { get; }
        public ReactiveCommand<Unit, Unit> RightCommand { get; }
    }
}