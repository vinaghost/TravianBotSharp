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
            var node = doc.GetElementbyId($"contract_building{(int)building}");
            if (node is null) return null;

            var button = node
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("new"));

            return button;
        }

        public static HtmlNode GetSpecialUpgradeButton(HtmlDocument doc)
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

        public static HtmlNode GetUpgradeButton(HtmlDocument doc)
        {
            var node = doc.DocumentNode.Descendants("div")
               .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            if (node is null) return null;

            var button = node.Descendants("button")
                .FirstOrDefault(x => x.HasClass("build"));

            return button;
        }
    }
}