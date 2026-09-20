using Microsoft.Playwright;

namespace MainCore.Parsers
{
    public static class NavigationBarParser
    {
        public static ILocator GetDorfButton(IPage page, int dorf)
        {
            return dorf switch
            {
                1 => GetResourceButton(page),
                _ => GetBuildingButton(page),
            };
        }

        private static ILocator GetButton(IPage page, int key)
        {
            var button = page.Locator($"#navigation a[accesskey='{key}']");
            return button;
        }

        private static ILocator GetResourceButton(IPage page) => GetButton(page, 1);

        private static ILocator GetBuildingButton(IPage page) => GetButton(page, 2);
    }
}