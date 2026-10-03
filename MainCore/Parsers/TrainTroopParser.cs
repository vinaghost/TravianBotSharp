using System.Net;

namespace MainCore.Parsers
{
    public static class TrainTroopParser
    {
        public static ILocator GetInputBox(IPage page, TroopEnums troop)
        {
            var node = page.Locator($"div.troop:not(.empty):has(img.unit.u{(int)troop}) div.cta input.text");
            return node;
        }

        public static async Task<int> GetMaxAmount(IPage page, TroopEnums troop)
        {
            var node = page.Locator($"div.troop:not(.empty):has(img.unit.u{(int)troop}) div.cta a");
            var valueStrFixed = WebUtility.HtmlDecode(await node.InnerTextAsync());
            return valueStrFixed.ParseInt();
        }

        public static ILocator GetTrainButton(IPage page)
        {
            var node = page.Locator("#s1");
            return node;
        }
    }
}