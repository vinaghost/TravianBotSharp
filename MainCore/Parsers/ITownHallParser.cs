using HtmlAgilityPack;

namespace MainCore.Parsers
{
    public interface ITownHallParser
    {
        HtmlNode GetHoldButton(HtmlDocument doc, bool big);
    }
}