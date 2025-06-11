using System.Net;
using System.Text.RegularExpressions;
using MainCore.DTO;

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

        public static ProductionDto GetProduction(HtmlDocument doc)
        {
            var script = doc.DocumentNode
                .Descendants("script")
                .FirstOrDefault(x => x.InnerText.Contains("resources = {"));
            if (script is null) return new();

            var match = Regex.Match(script.InnerText, @"production\s*:\s*\{(?<data>[^}]*)\}", RegexOptions.Singleline);
            if (!match.Success) return new();

            var data = match.Groups["data"].Value;

            long Parse(string key)
            {
                var m = Regex.Match(data, $"\"{key}\"\\s*:\\s*(?<v>[-0-9,]+)");
                if (!m.Success) return -1;
                return m.Groups["v"].Value.ParseLong();
            }

            return new ProductionDto
            {
                Wood = Parse("l1"),
                Clay = Parse("l2"),
                Iron = Parse("l3"),
                Crop = Parse("l4"),
            };
        }
    }
}