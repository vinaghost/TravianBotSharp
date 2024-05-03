using HtmlAgilityPack;
using MainCore.DTO;
using System.Net;

namespace MainCore.Parsers.StockBarParser
{
    
    public class TTWars : IStockBarParser
    {
        public StorageDto Get(HtmlDocument doc)
        {
            var storage = new StorageDto()
            {
                Wood = GetWood(doc),
                Clay = GetClay(doc),
                Iron = GetIron(doc),
                Crop = GetCrop(doc),
                FreeCrop = GetFreeCrop(doc),
                Warehouse = GetWarehouseCapacity(doc),
                Granary = GetGranaryCapacity(doc)
            };
            return storage;
        }

        private static long GetResource(HtmlDocument doc, string id)
        {
            var node = doc.GetElementbyId(id);
            if (node is null) return -1;
            var valueStrFixed = WebUtility.HtmlDecode(node.InnerText);
            if (string.IsNullOrEmpty(valueStrFixed)) return -1;
            var valueStr = new string(valueStrFixed.Where(c => char.IsDigit(c)).ToArray());
            if (string.IsNullOrEmpty(valueStr)) return -1;
            return long.Parse(valueStr);
        }

        private static long GetWood(HtmlDocument doc) => GetResource(doc, "l1");

        private static long GetClay(HtmlDocument doc) => GetResource(doc, "l2");

        private static long GetIron(HtmlDocument doc) => GetResource(doc, "l3");

        private static long GetCrop(HtmlDocument doc) => GetResource(doc, "l4");

        private static long GetFreeCrop(HtmlDocument doc) => GetResource(doc, "stockBarFreeCrop");

        private static long GetWarehouseCapacity(HtmlDocument doc)
        {
            var stockBarNode = doc.GetElementbyId("stockBar");
            if (stockBarNode is null) return -1;
            var warehouseNode = stockBarNode.Descendants("div").FirstOrDefault(x => x.HasClass("warehouse"));
            if (warehouseNode is null) return -1;
            var capacityNode = warehouseNode.Descendants("div").FirstOrDefault(x => x.HasClass("capacity"));
            if (capacityNode is null) return -1;
            var valueNode = capacityNode.Descendants("div").FirstOrDefault(x => x.HasClass("value"));
            if (valueNode is null) return -1;
            var valueStrFixed = WebUtility.HtmlDecode(valueNode.InnerText);
            if (string.IsNullOrEmpty(valueStrFixed)) return -1;
            var valueStr = new string(valueStrFixed.Where(c => char.IsDigit(c)).ToArray());
            if (string.IsNullOrEmpty(valueStr)) return -1;
            return long.Parse(valueStr);
        }

        private static long GetGranaryCapacity(HtmlDocument doc)
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
            var valueStr = new string(valueStrFixed.Where(c => char.IsDigit(c)).ToArray());
            if (string.IsNullOrEmpty(valueStr)) return -1;
            return long.Parse(valueStr);
        }
    }
}