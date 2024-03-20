using HtmlAgilityPack;
using MainCore.Common.Enums;

namespace MainCore.Parsers
{
    public interface IAcademyParser
    {
        HtmlNode GetResearchButton(HtmlDocument doc, TroopEnums troop);
    }
}