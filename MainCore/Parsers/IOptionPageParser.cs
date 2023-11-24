using HtmlAgilityPack;

namespace MainCore.Parsers
{
    public interface IOptionPageParser
    {
        HtmlNode GetHideContextualHelpOption(HtmlDocument doc);
        HtmlNode GetMovementsPerPageOption(HtmlDocument doc);
        HtmlNode GetOptionButton(HtmlDocument doc);
        HtmlNode GetReporPerPageOption(HtmlDocument doc);
        HtmlNode GetSubmitButton(HtmlDocument doc);
        bool IsContextualHelpShow(HtmlDocument doc);
    }
}