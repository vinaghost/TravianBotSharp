namespace MainCore.Parsers
{
    public static class UpgradeParser
    {
        public static async Task<ILocator> GetPanel(IPage page, BuildingEnums building)
        {
            if (await IsEmptySite(page))
            {
                return page.Locator($"#contract_building{(int)building} div.upgradeBuilding");
            }
            else
            {
                return page.Locator("div.upgradeBuilding");
            }
        }

        public static async Task<ILocator> GetRequiredResource(IPage page, BuildingEnums building)
        {
            var panel = await GetPanel(page, building);
            var nodes = panel.Locator("#contract div.resourceWrapper div.resource");
            return nodes;
        }

        public static async Task<TimeSpan> GetTimeWhenEnoughResource(IPage page, BuildingEnums building)
        {
            var panel = await GetPanel(page, building);
            var node = panel.Locator("#contract div.errorMessage span.timer");
            var timeValue = await node.GetAttributeAsync("value");
            return TimeSpan.FromSeconds(int.Parse(timeValue ?? "0"));
        }

        public static async Task<ILocator> GetFillUpButton(IPage page, BuildingEnums building)
        {
            var panel = await GetPanel(page, building);
            var node = panel.Locator("#contract .resourceWrapper .resource.transfer.fillUp");
            return node.First;
        }

        public static async Task<ILocator> GetNormalButton(IPage page, BuildingEnums building)
        {
            if (await IsEmptySite(page))
            {
                return page.Locator($"#contract_building{(int)building} div.upgradeButtonsContainer .section1 button.new");
            }
            else
            {
                return page.Locator("div.upgradeButtonsContainer .section1 button.build");
            }
        }

        public static async Task<ILocator> GetSpecialButton(IPage page, BuildingEnums building)
        {
            if (await IsEmptySite(page))
            {
                return page.Locator($"#contract_building{(int)building} div.upgradeButtonsContainer .section2 button.videoFeatureButton");
            }
            else
            {
                return page.Locator("div.upgradeButtonsContainer .section2 button.videoFeatureButton");
            }
        }

        private static async Task<bool> IsEmptySite(IPage page)
        {
            var parentPanel = page.Locator("#build");
            var classes = await parentPanel.GetAttributeAsync("class");
            return classes is not null && classes.Contains("gid0");
        }
    }
}