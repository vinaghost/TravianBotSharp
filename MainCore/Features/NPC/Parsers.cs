namespace MainCore.Features.NPC;

public static class Parsers
{
    public static ILocator NpcDialog(this IPage page)
    {
        var dialog = page.Locator(".exchangeResources #npc");
        return dialog;
    }

    public static ILocator GetExchangeResourcesButton(this IPage page)
    {
        var button = page.Locator(".npcMerchant button.gold");
        return button;
    }

    public static ILocator GetDistributeButton(this IPage page)
    {
        var button = page.Locator(".exchangeResources p#submitText button.gold");
        return button;
    }

    public static ILocator GetRedeemButton(this IPage page)
    {
        var button = page.Locator("#npc_market_button");
        return button;
    }

    public static async Task<long> GetSum(this IPage page)
    {
        var sum = page.Locator("#sum");
        var sumText = await sum.InnerTextAsync();
        return sumText.ParseLong();
    }

    public static ILocator GetInputs(this IPage page)
    {
        var inputs = page.Locator(".exchangeResources #npc tbody td.sel input");
        return inputs;
    }
}