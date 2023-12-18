using HtmlAgilityPack;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers
{
    [RegisterAsTransient(Common.Enums.ServerEnums.TTWars)]
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

            return questmasterButton.HasClass("claimable");
        }

        public HtmlNode GetQuestCollectButton(HtmlDocument doc)
        {
            var taskTable = doc.DocumentNode
                .Descendants("div")
                .Where(x => x.HasClass("tasks") && x.HasClass("tasksVillage"))
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