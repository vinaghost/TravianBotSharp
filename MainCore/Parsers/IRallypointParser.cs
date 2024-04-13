using HtmlAgilityPack;
using MainCore.Common.Models;

namespace MainCore.Parsers
{
    public interface IRallypointParser
    {
        List<IncomingAttack> GetIncomingAttacks(HtmlDocument doc);
    }
}