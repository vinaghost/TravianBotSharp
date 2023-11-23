using HtmlAgilityPack;

namespace MainCore.Parsers
{
    public interface IBuildingParser
    {
        HtmlNode GetBuilding(HtmlDocument doc, int location);
    }
}