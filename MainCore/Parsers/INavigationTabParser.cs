using HtmlAgilityPack;

namespace MainCore.Parsers
{
    public interface INavigationTabParser
    {
        int CountTab(HtmlDocument doc);
        HtmlNode GetTab(HtmlDocument doc, int index);
        bool IsTabActive(HtmlNode node);
    }
}