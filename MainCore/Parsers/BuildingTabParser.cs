using Microsoft.Playwright;

namespace MainCore.Parsers
{
    public static class BuildingTabParser
    {
        public static ILocator GetTabs(IPage page)
        {
            var tabs = page.Locator(".contentNavi.subNavi a.tabItem");
            return tabs;
        }

        public static async Task<bool> IsTabActive(ILocator tab)
        {
            return await tab.EvaluateAsync<bool>("node => node.classList.contains('active')");
        }
    }
}