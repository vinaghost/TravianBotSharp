using System.Net;

namespace MainCore.Parsers
{
    public static class StorageParser
    {
        private static long GetResource(HtmlDocument doc, string id)
        {
            var node = doc.GetElementbyId(id);
            if (node is null) return -1;
            return node.InnerText.ParseLong();
        }

        public static long GetWood(HtmlDocument doc) => GetResource(doc, "l1");

        public static long GetClay(HtmlDocument doc) => GetResource(doc, "l2");

        public static long GetIron(HtmlDocument doc) => GetResource(doc, "l3");

        public static long GetCrop(HtmlDocument doc) => GetResource(doc, "l4");

        public static long GetFreeCrop(HtmlDocument doc) => GetResource(doc, "stockBarFreeCrop");

        public static long GetWarehouseCapacity(HtmlDocument doc)
        {
            var stockBarNode = doc.GetElementbyId("stockBar");
            if (stockBarNode is null) return -1;
            var warehouseNode = stockBarNode.Descendants("div").FirstOrDefault(x => x.HasClass("warehouse"));
            if (warehouseNode is null) return -1;
            var capacityNode = warehouseNode.Descendants("div").FirstOrDefault(x => x.HasClass("capacity"));
            if (capacityNode is null) return -1;
            var valueNode = capacityNode.Descendants("div").FirstOrDefault(x => x.HasClass("value"));
            if (valueNode is null) return -1;
            return valueNode.InnerText.ParseLong();
        }

        public static long GetGranaryCapacity(HtmlDocument doc)
        {
            var stockBarNode = doc.GetElementbyId("stockBar");
            if (stockBarNode is null) return -1;
            var granaryNode = stockBarNode.Descendants("div").FirstOrDefault(x => x.HasClass("granary"));
            if (granaryNode is null) return -1;
            var capacityNode = granaryNode.Descendants("div").FirstOrDefault(x => x.HasClass("capacity"));
            if (capacityNode is null) return -1;
            var valueNode = capacityNode.Descendants("div").FirstOrDefault(x => x.HasClass("value"));
            if (valueNode is null) return -1;
            var valueStrFixed = WebUtility.HtmlDecode(valueNode.InnerText);
            if (string.IsNullOrEmpty(valueStrFixed)) return -1;
            return valueNode.InnerText.ParseLong();
        }
    }
}
