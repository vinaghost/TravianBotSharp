namespace MainCore.Commands.Abstract
{
    public abstract class QuestCommand
    {
        protected static HtmlNode GetQuestMaster(HtmlDocument doc)
        {
            var questmasterButton = doc.GetElementbyId("questmasterButton");
            return questmasterButton;
        }

        protected static bool IsQuestClaimable(HtmlDocument doc)
        {
            var questmasterButton = GetQuestMaster(doc);
            if (questmasterButton is null) return false;
            var newQuestSpeechBubble = questmasterButton
                .Descendants("div")
                .Where(x => x.HasClass("newQuestSpeechBubble"))
                .Any();
            return newQuestSpeechBubble;
        }
    }
}