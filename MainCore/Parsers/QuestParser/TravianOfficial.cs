using HtmlAgilityPack;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.QuestParser
{
    [RegisterAsTransient(Common.Enums.ServerEnums.TravianOfficial)]
    public class TravianOfficial : IQuestParser
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
                .Where(x => x.HasClass("newQuestSpeechBubble"))
                .Any();
            return newQuestSpeechBubble;
        }

        public HtmlNode GetQuestCollectButton(HtmlDocument doc)
        {
            var taskTable = doc.DocumentNode
                .Descendants("div")
                .Where(x => x.HasClass("taskOverview"))
                .FirstOrDefault();
            if (taskTable is null) return null;

            var button = taskTable
                .Descendants("button")
                .Where(x => x.HasClass("collect") && !x.HasClass("disabled"))
                .FirstOrDefault();
            return button;
        }
    }
}