using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.DTO;

namespace MainCore.Parsers
{
    public interface ISettleParser
    {
        IEnumerable<ExpansionSlotDto> Get(HtmlDocument doc);

        int GetProgressingSettlerAmount(HtmlDocument doc, TroopEnums troop);

        TimeSpan GetProgressingSettlerCompleteTime(HtmlDocument doc, TroopEnums troop);

        DateTime GetSettleArrivalTime(HtmlDocument doc);

        HtmlNode GetSettleButton(HtmlDocument doc);

        int GetSettlerAmount(HtmlDocument doc, TroopEnums troop);

        bool IsSettlerEnough(HtmlDocument doc, TroopEnums troop);
    }
}