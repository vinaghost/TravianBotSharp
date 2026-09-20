namespace MainCore.Parsers
{
    public static class QuestParser
    {
        public static ILocator GetQuestMaster(IPage page)
        {
            var button = page.Locator("#questmasterButton");
            return button;
        }

        public static ILocator IsQuestClaimable(IPage page)
        {
            var speechBubble = page.Locator("#questmasterButton div.newQuestSpeechBubble");
            return speechBubble;
        }

        public static ILocator GetQuestCollectButton(IPage page)
        {
            var buttons = page.Locator("div.tasks.tasksVillage div.taskOverview button.collect:not(.disabled)");
            return buttons;
        }

        public static ILocator IsQuestPage(IPage page)
        {
            var table = page.Locator("div.tasks.tasksVillage div.taskOverview");
            return table;
        }
    }
}