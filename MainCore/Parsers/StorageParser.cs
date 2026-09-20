using System.Net;

namespace MainCore.Parsers
{
    public static class StorageParser
    {
        private static async Task<long> GetResource(IPage page, string id)
        {
            var valueNode = page.Locator($"#{id}");
            var valueStrFixed = WebUtility.HtmlDecode(await valueNode.InnerTextAsync());
            return valueStrFixed.ParseLong();
        }

        public static Task<long> GetWood(IPage page) => GetResource(page, "l1");

        public static Task<long> GetClay(IPage page) => GetResource(page, "l2");

        public static Task<long> GetIron(IPage page) => GetResource(page, "l3");

        public static Task<long> GetCrop(IPage page) => GetResource(page, "l4");

        public static Task<long> GetFreeCrop(IPage page) => GetResource(page, "stockBarFreeCrop");

        public static async Task<long> GetWarehouseCapacity(IPage page)
        {
            var valueNode = page.Locator("#stockBar div.warehouse div.capacity div.value");
            var valueStrFixed = WebUtility.HtmlDecode(await valueNode.InnerTextAsync());
            return valueStrFixed.ParseLong();
        }

        public static async Task<long> GetGranaryCapacity(IPage page)
        {
            var valueNode = page.Locator("#stockBar div.granary div.capacity div.value");
            var valueStrFixed = WebUtility.HtmlDecode(await valueNode.InnerTextAsync());
            return valueStrFixed.ParseLong();
        }
    }
}