namespace MainCore.Parsers
{
    public static class UpgradeParser
    {
        private static ILocator GetContractNode(IPage page, BuildingEnums building)
        {
            var node = page.Locator($"#contract_building{(int)building}"); // site
            node ??= page.Locator("#contract"); // building
            return node;
        }

        public static ILocator GetRequiredResource(IPage page, BuildingEnums building)
        {
            var nodes = GetContractNode(page, building)
                .Locator("div.resourceWrapper div.resource");
            return nodes;
        }

        public static async Task<TimeSpan> GetTimeWhenEnoughResource(IPage page, BuildingEnums building)
        {
            var node = GetContractNode(page, building)
                .Locator("div.errorMessage span.timer");
            var timeValue = await node.GetAttributeAsync("value");
            return TimeSpan.FromSeconds(int.Parse(timeValue ?? "0"));
        }

        public static ILocator GetConstructButton(IPage page, BuildingEnums building)
        {
            if (building.IsResourceField()) return GetUpgradeButton(page);

            var button = page.Locator($"#contract_building{(int)building} button.new");
            return button;
        }

        public static ILocator GetSpecialUpgradeButton(IPage page)
        {
            var button = page.Locator("div.upgradeButtonsContainer button.videoFeatureButton.green");
            return button;
        }

        public static ILocator GetUpgradeButton(IPage page)
        {
            var button = page.Locator("div.upgradeButtonsContainer button.build");
            return button;
        }
    }
}