namespace MainCore.Parsers
{
    public static class UpgradeParser
    {
        private static HtmlNode GetContractNode(HtmlDocument doc, BuildingEnums building)
        {
            var node = doc.GetElementbyId($"contract_building{(int)building}"); // building
            node ??= doc.GetElementbyId("contract"); // site
            return node;
        }

        public static List<HtmlNode> GetRequiredResource(HtmlDocument doc, BuildingEnums building)
        {
            var node = GetContractNode(doc, building);

            if (node is null) return [];
            var resourceWrapper = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("resourceWrapper"));
            if (resourceWrapper is null) return [];

            var resources = resourceWrapper.Descendants("div")
                .Where(x => x.HasClass("resource"))
                .ToList();

            if (resources.Count != 5) return [];
            return resources;
        }

        public static TimeSpan GetTimeWhenEnoughResource(HtmlDocument doc, BuildingEnums building)
        {
            var node = GetContractNode(doc, building);

            if (node is null) return TimeSpan.Zero;

            var errorMessage = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("errorMessage"));
            if (errorMessage is null) return TimeSpan.Zero;
            var timer = errorMessage.Descendants("span")
                .FirstOrDefault(x => x.HasClass("timer"));
            if (timer is null) return TimeSpan.Zero;
            var time = timer.GetAttributeValue("value", 0);
            return TimeSpan.FromSeconds(time);
        }

        public static HtmlNode GetConstructButton(HtmlDocument doc, BuildingEnums building)
        {
            if (building.IsResourceField()) return GetUpgradeButton(doc);

            var contract_building = doc.GetElementbyId($"contract_building{(int)building}");
            BrokenParserException.ThrowIfNull(contract_building);

            var button = contract_building
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("new"));

            BrokenParserException.ThrowIfNull(button);
            return button;
        }

        public static HtmlNode GetSpecialUpgradeButton(HtmlDocument doc)
        {
            var upgradeButtonsContainer = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            BrokenParserException.ThrowIfNull(upgradeButtonsContainer);

            var button = upgradeButtonsContainer
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("videoFeatureButton") && x.HasClass("green"));
            BrokenParserException.ThrowIfNull(button);

            return button;
        }

        public static HtmlNode GetUpgradeButton(HtmlDocument doc)
        {
            var upgradeButtonsContainer = doc.DocumentNode.Descendants("div")
               .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            BrokenParserException.ThrowIfNull(upgradeButtonsContainer);

            var button = upgradeButtonsContainer.Descendants("button")
                .FirstOrDefault(x => x.HasClass("build"));
            BrokenParserException.ThrowIfNull(button);

            return button;
        }
    }
}