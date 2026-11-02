namespace MainCore.Commands.Features.ClaimDailyQuest
{
    [Handler]
    public sealed partial class CheckDailyQuestCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => $"{nameof(CheckDailyQuestCommand)}";
        }

        private async ValueTask<bool> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            var canClaimQuest = await QuestParser.IsDailyQuestClaimable(browser.CurrentPage);
            return canClaimQuest;
        }
    }
}