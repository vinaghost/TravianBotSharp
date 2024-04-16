using HtmlAgilityPack;
using MainCore.Common.Models;

namespace MainCore.Parsers
{
    public interface IRallypointParser
    {
        HtmlNode GetRaidInput(HtmlDocument doc);

        HtmlNode GetCancelButton(HtmlDocument doc);

        HtmlNode GetConfirmButton(HtmlDocument doc);

        List<IncomingAttack> GetIncomingAttacks(HtmlDocument doc);

        int GetTroopAmount(HtmlNode input);

        IEnumerable<HtmlNode> GetTroopInput(HtmlDocument doc);

        HtmlNode GetXInput(HtmlDocument doc);

        HtmlNode GetYInput(HtmlDocument doc);

        HtmlNode GetSendButton(HtmlDocument doc);
    }
}