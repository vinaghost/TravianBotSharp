using HtmlAgilityPack;

namespace MainCore.Parsers
{
    
    public class TTWars : IQuestParser
    {
        public HtmlNode GetQuestMaster(HtmlDocument doc)
        {
            var questmasterButton = doc.GetElementbyId("questmasterButton");
            return questmasterButton;
        }

        public bool IsQuestClaimable(HtmlDocument doc)
        {
            var questmasterButton = GetQuestMaster(doc);
            if (questmasterButton is null) return false;

            var newQuestSpeechBubble = questmasterButton
                .Descendants("div")
                .Any(x => x.HasClass("newQuestSpeechBubble"));
            return newQuestSpeechBubble;
        }

        public HtmlNode GetQuestCollectButton(HtmlDocument doc)
        {
            var button = doc.DocumentNode
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("questButtonNext"));
            return button;
        }
    }
}