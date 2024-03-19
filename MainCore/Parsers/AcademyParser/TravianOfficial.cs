using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.AcademyParser
{
    [RegisterAsTransient(ServerEnums.TravianOfficial)]
    public class TravianOfficial : IAcademyParser
    {
        public HtmlNode GetResearchButton(HtmlDocument doc, TroopEnums troop)
        {
            var troopNode = doc.DocumentNode
                .Descendants("img")
                .FirstOrDefault(x => x.HasClass("u" + (int)troop));

            if (troopNode is null) return null;

            while (!troopNode.HasClass("research")) troopNode = troopNode.ParentNode;
            var button = troopNode
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("green"));
            return button;
        }
    }
}