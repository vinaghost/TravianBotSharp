using System.Net;

namespace MainCore.Features.TrainTroop;

public static class Parsers
{
    public static ILocator GetInputBox(this IPage page, TroopEnums troop)
    {
        var node = page.Locator($"div.troop:not(.empty):has(img.unit.u{(int)troop}) div.cta input.text");
        return node;
    }

    public static async Task<int> GetMaxAmount(this IPage page, TroopEnums troop)
    {
        var node = page.Locator($"div.troop:not(.empty):has(img.unit.u{(int)troop}) div.cta a");
        var valueStrFixed = WebUtility.HtmlDecode(await node.InnerTextAsync());
        return valueStrFixed.ParseInt();
    }

    public static ILocator GetTrainButton(this IPage page)
    {
        var node = page.Locator("#s1");
        return node;
    }
}