using HtmlAgilityPack;
using MainCore.DTO;

namespace MainCore.Parsers
{
    public interface ISettleParser
    {
        IEnumerable<ExpansionSlotDto> Get(HtmlDocument doc);
    }
}