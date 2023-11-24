using HtmlAgilityPack;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.OptionPageParser
{
    [RegisterAsTransient(Common.Enums.ServerEnums.TTWars)]
    public class TTWars : IOptionPageParser
    {
        public bool IsContextualHelpShow(HtmlDocument doc)
        {
            var node = doc.GetElementbyId("contextualHelp");
            return node is not null;
        }

        public HtmlNode GetOptionButton(HtmlDocument doc)
        {
            var outOfGame = doc.GetElementbyId("outOfGame");
            if (outOfGame is null) return null;
            var li = outOfGame.Descendants("li").Where(x => x.HasClass("options")).FirstOrDefault();
            if (li is null) return null;
            var a = li.Descendants("a").FirstOrDefault();
            return a;
        }

        public HtmlNode GetHideContextualHelpOption(HtmlDocument doc)
        {
            var node = doc.GetElementbyId("hideContextualHelp");
            return node;
        }

        public HtmlNode GetReporPerPageOption(HtmlDocument doc)
        {
            var node = doc.GetElementbyId("epp");
            return node;
        }

        public HtmlNode GetMovementsPerPageOption(HtmlDocument doc)
        {
            var node = doc.GetElementbyId("troopMovementsPerPage");
            return node;
        }

        public HtmlNode GetSubmitButton(HtmlDocument doc)
        {
            var div = doc.DocumentNode.Descendants("div").Where(x => x.HasClass("submitButtonContainer")).FirstOrDefault();
            if (div is null) return null;
            var button = div.Descendants("button").FirstOrDefault();
            return button;
        }
    }
}