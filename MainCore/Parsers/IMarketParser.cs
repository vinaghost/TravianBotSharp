using HtmlAgilityPack;

namespace MainCore.Parsers
{
    public interface IMarketParser
    {
        HtmlNode GetDistributeButton(HtmlDocument doc);
        HtmlNode GetExchangeResourcesButton(HtmlDocument doc);
        IEnumerable<HtmlNode> GetInputs(HtmlDocument doc);
        HtmlNode GetRedeemButton(HtmlDocument doc);
        long GetSum(HtmlDocument doc);
        bool NPCDialogShown(HtmlDocument doc);
    }
}