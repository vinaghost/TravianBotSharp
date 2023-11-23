using HtmlAgilityPack;

namespace MainCore.Parsers
{
    public interface INavigationBarParser
    {
        HtmlNode GetBuildingButton(HtmlDocument doc);
        HtmlNode GetDailyButton(HtmlDocument doc);
        HtmlNode GetMapButton(HtmlDocument doc);
        HtmlNode GetMessageButton(HtmlDocument doc);
        HtmlNode GetReportsButton(HtmlDocument doc);
        HtmlNode GetResourceButton(HtmlDocument doc);
        HtmlNode GetStatisticsButton(HtmlDocument doc);
    }
}