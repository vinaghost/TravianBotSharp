using MainCore.Commands.UI.Villages.AttackViewModel;
using MainCore.Models;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton<AttackViewModel>]
    public partial class AttackViewModel : VillageTabViewModelBase
    {
        public AttackInput AttackInput { get; } = new();
        private readonly IDialogService _dialogService;
        private readonly ICustomServiceScopeFactory _serviceScopeFactory;

        public AttackViewModel(IDialogService dialogService, ICustomServiceScopeFactory serviceScopeFactory)
        {
            _dialogService = dialogService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        [ReactiveCommand]
        private async Task Send()
        {
            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var handler = scope.ServiceProvider.GetRequiredService<SendAttackCommand.Handler>();
            var (x, y, type, troops) = AttackInput.Get();
            var plan = new AttackPlan { X = x, Y = y, Type = type, Troops = troops };
            var result = await handler.HandleAsync(new(AccountId, VillageId, plan));
            if (result.IsFailed)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
            }
            else
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Attack sent"));
            }
        }

        protected override Task Load(VillageId villageId)
        {
            // no async load for now
            return Task.CompletedTask;
        }
    }
}
