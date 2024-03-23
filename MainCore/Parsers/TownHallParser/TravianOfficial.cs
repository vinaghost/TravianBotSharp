using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.TownHallParser
{
    [RegisterAsTransient(ServerEnums.TravianOfficial)]
    public class TravianOfficial : ITownHallParser
    {
        public HtmlNode GetHoldButton(HtmlDocument doc, bool big)
        {
            var box = doc.DocumentNode
                .Descendants("div")
                .Where(x => x.HasClass("build_details") && x.HasClass("researches"))
                .FirstOrDefault();
            if (box is null) return null;

            var buttons = box
                .Descendants("button")
                .Where(x => x.HasClass("textButtonV1") && x.HasClass("green"))
                .ToList();

            if (buttons.Count == 0) return null;

            if (big)
            {
                if (buttons.Count == 1) return null;
                return buttons[1];
            }

            return buttons[0];
        }
    }
}