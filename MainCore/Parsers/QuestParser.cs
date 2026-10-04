namespace MainCore.Parsers
{
    public static class QuestParser
    {
        public static ILocator GetQuestMaster(IPage page)
        {
            var button = page.Locator("#questmasterButton");
            return button;
        }

        public static async Task<bool> IsQuestClaimable(IPage page)
        {
            var speechBubble = page.Locator("#questmasterButton div.newQuestSpeechBubble");
            return await speechBubble.CountAsync() > 0;
        }

        public static async Task<bool> IsDailyQuestClaimable(IPage page)
        {
            var indicator = page.Locator("#navigation .dailyQuests .indicator:not(.hidden)");
            return await indicator.CountAsync() > 0;
        }

        public static ILocator GetQuestCollectButton(IPage page)
        {
            var buttons = page.Locator("div.taskOverview button.collect:not(.disabled)");
            return buttons;
        }

        public static ILocator GetDailyQuestCollectButton(IPage page)
        {
            var buttons = page.Locator("#dailyQuests .screen.active .dailyProgress div.reward:has(div.rewardImage.achieved)");
            return buttons;
        }
    }
}