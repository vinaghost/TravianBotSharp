using HtmlAgilityPack;
using MainCore.Common.Enums;

namespace MainCore.Parsers
{
    public interface ITroopPageParser
    {
        HtmlNode GetInputBox(HtmlDocument doc, TroopEnums troop);

        int GetMaxAmount(HtmlDocument doc, TroopEnums troop);

        TimeSpan GetQueueTrainTime(HtmlDocument doc);

        HtmlNode GetTrainButton(HtmlDocument doc);

        long[] GetTrainCost(HtmlDocument doc, TroopEnums troop);

        TimeSpan GetTrainTime(HtmlDocument doc, TroopEnums troop);
    }
}