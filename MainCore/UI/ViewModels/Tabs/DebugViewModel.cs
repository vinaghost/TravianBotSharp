using MainCore.Commands.UI.DebugViewModel;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<DebugViewModel>]
    public partial class DebugViewModel : AccountTabViewModelBase
    {
        public ObservableCollection<TaskItem> Tasks { get; } = [];

        [Reactive]
        private string _logs;

        [Reactive]
        private string _endpointAddress;

        public DebugViewModel(LogSink logSink, ITaskManager taskManager)
        {
            logSink.LogEmitted += LogEmitted;
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

        public void TaskListRefresh(AccountId accountId)
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
        private async Task<List<TaskItem>> LoadTask(AccountId accountId)
        {
            var serviceScopeFactory = Locator.Current.GetService<IServiceScopeFactory>();
            using var scope = serviceScopeFactory.CreateScope();
            var getTaskItemsQuery = scope.ServiceProvider.GetRequiredService<GetTaskItemsQuery.Handler>();
            var tasks = await getTaskItemsQuery.HandleAsync(new(accountId), CancellationToken.None);
            return tasks;
        }

        [ReactiveCommand]
        private async Task<string> LoadLog(AccountId accountId)
        {
            var serviceScopeFactory = Locator.Current.GetService<IServiceScopeFactory>();
            using var scope = serviceScopeFactory.CreateScope();
            var getLogQuery = scope.ServiceProvider.GetRequiredService<GetLogQuery.Handler>();
            var log = await getLogQuery.HandleAsync(new(accountId), CancellationToken.None);
            return log;
        }

        [ReactiveCommand]
        private async Task<string> LoadEndpointAddress(AccountId accountId)
        {
            var serviceScopeFactory = Locator.Current.GetService<IServiceScopeFactory>();
            using var scope = serviceScopeFactory.CreateScope();
            var getEndpointAdressQuery = scope.ServiceProvider.GetRequiredService<GetEndpointAdressQuery.Handler>();
            var address = await getEndpointAdressQuery.HandleAsync(new(accountId), CancellationToken.None);
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