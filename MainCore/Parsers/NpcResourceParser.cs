using Microsoft.Playwright;

namespace MainCore.Parsers
{
    public static class NpcResourceParser
    {
        public static ILocator NpcDialog(IPage page)
        {
            var dialog = page.Locator(".exchangeResources #npc");
            return dialog;
        }

        public static ILocator GetExchangeResourcesButton(IPage page)
        {
            var button = page.Locator(".npcMerchant button.gold");
            return button;
        }

        public static ILocator GetDistributeButton(IPage page)
        {
            var button = page.Locator(".exchangeResources p#submitText button.gold");
            return button;
        }

        public static ILocator GetRedeemButton(IPage page)
        {
            var button = page.Locator("#npc_market_button");
            return button;
        }

        public static async Task<long> GetSum(IPage page)
        {
            var sum = page.Locator("#sum");
            var sumText = await sum.InnerTextAsync();
            return sumText.ParseLong();
        }

        public static ILocator GetInputs(IPage page)
        {
            var inputs = page.Locator(".exchangeResources #npc tbody td.sel input");
            return inputs;
        }
    }
}