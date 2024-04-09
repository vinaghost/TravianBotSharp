using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Reactive;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class AlertViewModel : AccountTabViewModelBase
    {
        public ReactiveCommand<Unit, Unit> Test { get; }

        protected override Task Load(AccountId accountId)
        {
            return Task.CompletedTask;
        }

        private bool _discordEnable;
        private string _discordWebhookUrl;

        public AlertViewModel()
        {
            Test = ReactiveCommand.CreateFromTask(TestHandler);
        }

        private async Task TestHandler()
        {
            await Task.CompletedTask;
        }

        public bool DiscordEnable
        {
            get => _discordEnable;
            set => this.RaiseAndSetIfChanged(ref _discordEnable, value);
        }

        public string DiscordWebhookUrl
        {
            get => _discordWebhookUrl;
            set => this.RaiseAndSetIfChanged(ref _discordWebhookUrl, value);
        }
    }
}