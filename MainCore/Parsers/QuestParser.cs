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

        public static ILocator GetQuestCollectButton(IPage page)
        {
            var buttons = page.Locator("div.tasks.tasksVillage div.taskOverview button.collect:not(.disabled)");
            return buttons;
        }

        public static async Task<bool> IsQuestPage(IPage page)
        {
            var table = page.Locator("div.tasks.tasksVillage div.taskOverview");
            var count = await table.CountAsync();
            return count > 0;
        }
    }
}