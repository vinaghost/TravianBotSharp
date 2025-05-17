namespace MainCore.Parsers
{
    public static class VillagePanelParser
    {
        public static HtmlNode? GetVillageNode(HtmlDocument doc, VillageId villageId)
        {
            var sidebarBoxVillagelist = doc.GetElementbyId("sidebarBoxVillagelist");
            if (sidebarBoxVillagelist is null) return null;
            var villages = sidebarBoxVillagelist
                .Descendants("div")
                .Where(x => x.HasClass("listEntry"))
                .ToList();

            var village = villages.FirstOrDefault(x => GetId(x) == villageId);
            return village;
        }

        public static VillageId GetCurrentVillageId(HtmlDocument doc)
        {
            var sidebarBoxVillagelist = doc.GetElementbyId("sidebarBoxVillagelist");
            if (sidebarBoxVillagelist is null) return default;
            var village = sidebarBoxVillagelist
                .Descendants("div")
                .Where(x => x.HasClass("listEntry"))
                .Where(x => IsActive(x))
                .Select(x => GetId(x))
                .FirstOrDefault();
            return village;
        }

        public static bool IsActive(HtmlNode node)
        {
            return node.HasClass("active");
        }

        private static VillageId GetId(HtmlNode node)
        {
            var dataDid = node.GetAttributeValue("data-did", 0);
            return new VillageId(dataDid);
        }

        public static IEnumerable<VillageDto> Get(HtmlDocument doc)
        {
            var nodes = GetVillages(doc);
            foreach (var node in nodes)
            {
                var id = GetId(node);
                var name = GetName(node);
                var x = GetX(node);
                var y = GetY(node);
                var isActive = IsActive(node);
                var isUnderAttack = IsUnderAttack(node);
                yield return new()
                {
                    Id = id,
                    Name = name,
                    X = x,
                    Y = y,
                    IsActive = isActive,
                    IsUnderAttack = isUnderAttack,
                };
            }
        }

        private static List<HtmlNode> GetVillages(HtmlDocument doc)
        {
            var sidebarBoxVillagelist = doc.GetElementbyId("sidebarBoxVillagelist");
            if (sidebarBoxVillagelist is null) return [];
            var villages = sidebarBoxVillagelist
                .Descendants("div")
                .Where(x => x.HasClass("listEntry") && x.HasClass("village"))
                .ToList();
            return villages;
        }

        private static bool IsUnderAttack(HtmlNode node)
        {
            return node.HasClass("attack");
        }

        private static string GetName(HtmlNode node)
        {
            var textNode = node
                .Descendants("a")
                .FirstOrDefault();
            if (textNode is null) return "";
            var nameNode = textNode
                .Descendants("span")
                .FirstOrDefault(x => x.HasClass("name"));
            if (nameNode is null) return "";
            return nameNode.InnerText;
        }

        private static int GetX(HtmlNode node)
        {
            var xNode = node
                .Descendants("span")
                .FirstOrDefault(x => x.HasClass("coordinateX"));
            if (xNode is null) return 0;
            return xNode.InnerText.ParseInt();
        }

        private static int GetY(HtmlNode node)
        {
            var yNode = node
                .Descendants("span")
                .FirstOrDefault(x => x.HasClass("coordinateY"));
            if (yNode is null) return 0;
            return yNode.InnerText.ParseInt();
        }
    }
}