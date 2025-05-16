namespace MainCore.Parsers
{
    public static class QuestParser
    {
        public static HtmlNode GetQuestMaster(HtmlDocument doc)
        {
            var questmasterButton = doc.GetElementbyId("questmasterButton");
            return questmasterButton;
        }

        public static bool IsQuestClaimable(HtmlDocument doc)
        {
            var questmasterButton = GetQuestMaster(doc);
            if (questmasterButton is null) return false;
            var newQuestSpeechBubble = questmasterButton
                .Descendants("div")
                .Any(x => x.HasClass("newQuestSpeechBubble"));
            return newQuestSpeechBubble;
        }

        public static HtmlNode GetQuestCollectButton(HtmlDocument doc)
        {
            var taskOverviewTable = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("taskOverview"));

            BrokenParserException.ThrowIfNull(taskOverviewTable);

            var collectButton = taskOverviewTable
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("collect") && !x.HasClass("disabled"));

            BrokenParserException.ThrowIfNull(collectButton);
            return collectButton;
        }

        public static bool IsQuestPage(HtmlDocument doc)
        {
            var table = doc.DocumentNode
                .Descendants("div")
                .Any(x => x.HasClass("tasks") && x.HasClass("tasksVillage"));
            return table;
        }
    }
}