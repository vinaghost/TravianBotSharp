namespace MainCore.Features.Navigate
{
    public static class Parser
    {
        public static ILocator GetTabs(this IPage page)
        {
            var tabs = page.Locator(".contentNavi.subNavi a.tabItem");
            return tabs;
        }

        public const string TabActiveExpression = "node => node.classList.contains('active')";

        public static async Task<bool> IsTabActive(this ILocator tab)
        {
            return await tab.EvaluateAsync<bool>(TabActiveExpression);
        }

        public static ILocator GetDorfButton(this IPage page, int dorf)
        {
            return dorf switch
            {
                1 => page.GetResourceButton(),
                _ => page.GetBuildingButton(),
            };
        }

        private static ILocator GetResourceButton(this IPage page) => page.GetButton(1);

        private static ILocator GetBuildingButton(this IPage page) => page.GetButton(2);

        public static ILocator GetVillageNode(this IPage page, VillageId villageId)
        {
            var node = page.Locator($"#sidebarBoxVillageList div.listEntry.village[data-did='{villageId}']");
            return node;
        }

        public static async Task<bool> IsActive(this ILocator locator)
        {
            return await locator.EvaluateAsync<bool>("node => node.classList.contains('active')");
        }

        public static ILocator GetFieldButton(this IPage page, int location)
        {
            var node = page.Locator($".village1 a.buildingSlot{location}");
            return node;
        }

        public static ILocator GetBuildingButton(this IPage page, int location)
        {
            var node = page.Locator($".village2 div.buildingSlot.a{location} svg path");
            return node.First;
        }

        public static async Task<int> GetBuildingLocation(this IPage page, BuildingEnums buildingType)
        {
            var buildingLocator = page.Locator($".village2 div.buildingSlot.g{buildingType}");
            if (await buildingLocator.CountAsync() == 0)
            {
                return -1;
            }
            var location = await buildingLocator.GetAttributeAsync("data-aid");
            return int.Parse(location ?? "-1");
        }
    }
}