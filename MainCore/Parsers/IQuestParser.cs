using HtmlAgilityPack;

namespace MainCore.Parsers
{
    public interface IQuestParser
    {
        HtmlNode GetQuestCollectButton(HtmlDocument doc);
        HtmlNode GetQuestMaster(HtmlDocument doc);
        bool IsQuestClaimable(HtmlDocument doc);
    }
}