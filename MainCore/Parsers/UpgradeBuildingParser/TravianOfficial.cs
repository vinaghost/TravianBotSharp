namespace MainCore.Parsers.UpgradeBuildingParser
{
    [RegisterAsParser]
    public class TravianOfficial : IUpgradeBuildingParser
    {
        public HtmlNode GetSpecialUpgradeButton(HtmlDocument doc)
        {
            var node = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            if (node is null) return null;

            var button = node
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("videoFeatureButton") && x.HasClass("green"));

            return button;
        }

        public HtmlNode GetUpgradeButton(HtmlDocument doc)
        {
            var node = doc.DocumentNode.Descendants("div")
               .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            if (node is null) return null;

            var button = node.Descendants("button")
                .FirstOrDefault(x => x.HasClass("build"));

            return button;
        }

        public HtmlNode GetConstructButton(HtmlDocument doc, BuildingEnums building)
        {
            var node = doc.GetElementbyId($"contract_building{(int)building}");
            if (node is null) return null;

            var button = node
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("new"));

            return button;
        }
    }
}