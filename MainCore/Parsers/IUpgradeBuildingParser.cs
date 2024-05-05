using HtmlAgilityPack;

namespace MainCore.Parsers
{
    public interface IUpgradeBuildingParser
    {
        HtmlNode GetConstructButton(HtmlDocument doc, BuildingEnums building = BuildingEnums.Site);

        long[] GetRequiredResource(HtmlDocument doc, bool isEmptySite, BuildingEnums building);

        HtmlNode GetSpecialUpgradeButton(HtmlDocument doc);

        TimeSpan GetTimeWhenEnoughResource(HtmlDocument doc, bool isEmptySite, BuildingEnums building = BuildingEnums.Site);

        HtmlNode GetUpgradeButton(HtmlDocument doc);
    }
}