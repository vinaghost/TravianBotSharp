namespace MainCore.Commands.Features.ClaimDailyQuest
{
    [Handler]
    public static partial class CheckDailyQuestCommand
    {
        public sealed record Command(IChromeBrowser Browser) : ICommand
        {
            public string Name => nameof(CheckDailyQuestCommand).Humanize();
        }

        private static async ValueTask<bool> HandleAsync(Command command)
        {
            var browser = command.Browser;
            var canClaimQuest = await QuestParser.IsDailyQuestClaimable(browser.CurrentPage);
            return canClaimQuest;
        }
    }
}