using HtmlAgilityPack;
using MainCore.Common.Enums;

namespace MainCore.Parsers
{
    public interface IAllianceParser
    {
        HtmlNode GetAllianceButton(HtmlDocument doc);
        IEnumerable<HtmlNode> GetBonusInputs(HtmlDocument doc);
        HtmlNode GetBonusSelector(HtmlDocument doc, AllianceBonusEnums bonus);
        HtmlNode GetContributeButton(HtmlDocument doc);
    }
}