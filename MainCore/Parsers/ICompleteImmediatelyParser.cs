using HtmlAgilityPack;

namespace MainCore.Parsers
{
    public interface ICompleteImmediatelyParser
    {
        HtmlNode GetCompleteButton(HtmlDocument doc);
        HtmlNode GetConfirmButton(HtmlDocument doc);
    }
}