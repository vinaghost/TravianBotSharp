using Microsoft.Playwright;

namespace MainCore.Parsers
{
    public static class CompleteImmediatelyParser
    {
        public static ILocator GetCompleteButton(IPage page)
        {
            var button = page.Locator(".buildingList .finishNow button");
            return button;
        }

        public static ILocator GetConfirmButton(IPage page)
        {
            var button = page.Locator("#finishNowDialog button");
            return button;
        }
    }
}