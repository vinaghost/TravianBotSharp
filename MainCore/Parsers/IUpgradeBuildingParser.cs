namespace MainCore.Parsers
{
    public interface IUpgradeBuildingParser
    {
        HtmlNode GetConstructButton(HtmlDocument doc, BuildingEnums building = BuildingEnums.Site);

        HtmlNode GetSpecialUpgradeButton(HtmlDocument doc);

        HtmlNode GetUpgradeButton(HtmlDocument doc);
    }
}