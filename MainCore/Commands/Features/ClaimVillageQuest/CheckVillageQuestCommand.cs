namespace MainCore.Commands.Features.ClaimVillageQuest
{
    [Handler]
    public static partial class CheckVillageQuestCommand
    {
        public sealed record Command(IChromeBrowser Browser) : ICommand
        {
            public string Name => nameof(CheckVillageQuestCommand).Humanize();
        }

        private static async ValueTask<bool> HandleAsync(Command command)
        {
            var browser = command.Browser;
            var canClaimQuest = await QuestParser.IsVillageQuestClaimable(browser.CurrentPage);
            return canClaimQuest;
        }
    }
}