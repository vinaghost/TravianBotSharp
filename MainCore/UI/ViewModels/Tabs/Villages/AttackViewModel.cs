using MainCore.Commands.UI.Villages.AttackViewModel;
using MainCore.Models;
using MainCore.Tasks;
using MainCore.Queries;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton<AttackViewModel>]
    public partial class AttackViewModel : VillageTabViewModelBase
    {
        public AttackInput AttackInput { get; } = new();
        private readonly IDialogService _dialogService;
        private readonly ICustomServiceScopeFactory _serviceScopeFactory;
        private readonly ITaskManager _taskManager;

        public AttackViewModel(IDialogService dialogService, ICustomServiceScopeFactory serviceScopeFactory, ITaskManager taskManager)
        {
            _dialogService = dialogService;
            _serviceScopeFactory = serviceScopeFactory;
            _taskManager = taskManager;
        }

        [ReactiveCommand]
        private async Task Send()
        {
            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var getVillageNameQuery = scope.ServiceProvider.GetRequiredService<GetVillageNameQuery.Handler>();
            var villageName = await getVillageNameQuery.HandleAsync(new(VillageId));

            var (x, y, type, troops, executeAt) = AttackInput.Get();
            var plan = new AttackPlan { X = x, Y = y, Type = type, Troops = troops };
            var task = new AttackTask.Task(AccountId, VillageId, villageName, plan, executeAt)
            {
                ExecuteAt = executeAt.AddSeconds(-20) > DateTime.Now ? executeAt.AddSeconds(-20) : DateTime.Now
            };
            _taskManager.Add(task);
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Attack task scheduled"));
        }

        protected override Task Load(VillageId villageId)
        {
            // no async load for now
            return Task.CompletedTask;
        }
    }
}
