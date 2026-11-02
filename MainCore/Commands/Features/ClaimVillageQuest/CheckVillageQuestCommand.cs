namespace MainCore.Commands.Features.ClaimVillageQuest
{
    [Handler]
    public sealed partial class CheckVillageQuestCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => $"{nameof(CheckVillageQuestCommand)}";
        }

        private async ValueTask<bool> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            var canClaimQuest = await QuestParser.IsVillageQuestClaimable(browser.CurrentPage);
            return canClaimQuest;
        }
    }
}