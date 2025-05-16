namespace MainCore.Parsers
{
    public static class VillagePanelParser
    {
        public static HtmlNode GetVillageNode(HtmlDocument doc, VillageId villageId)
        {
            var sidebarBoxVillagelist = doc.GetElementbyId("sidebarBoxVillagelist");
            BrokenParserException.ThrowIfNull(sidebarBoxVillagelist);
            var villages = sidebarBoxVillagelist
                .Descendants("div")
                .Where(x => x.HasClass("listEntry"))
                .ToList();
            BrokenParserException.ThrowIfEmpty(villages);

            var village = villages.First(x => GetId(x) == villageId);
            return village;
        }

        public static VillageId GetCurrentVillageId(HtmlDocument doc)
        {
            var sidebarBoxVillagelist = doc.GetElementbyId("sidebarBoxVillagelist");
            BrokenParserException.ThrowIfNull(sidebarBoxVillagelist);
            var village = sidebarBoxVillagelist
                .Descendants("div")
                .Where(x => x.HasClass("listEntry"))
                .Where(x => IsActive(x))
                .Select(x => GetId(x))
                .First();
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
            BrokenParserException.ThrowIfNull(sidebarBoxVillagelist);
            var villages = sidebarBoxVillagelist
                .Descendants("div")
                .Where(x => x.HasClass("listEntry") && x.HasClass("village"))
                .ToList();
            BrokenParserException.ThrowIfEmpty(villages);
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
            BrokenParserException.ThrowIfNull(textNode);
            var nameNode = textNode
                .Descendants("span")
                .FirstOrDefault(x => x.HasClass("name"));
            BrokenParserException.ThrowIfNull(nameNode);
            return nameNode.InnerText;
        }

        private static int GetX(HtmlNode node)
        {
            var xNode = node
                .Descendants("span")
                .FirstOrDefault(x => x.HasClass("coordinateX"));
            BrokenParserException.ThrowIfNull(xNode);
            return xNode.InnerText.ParseInt();
        }

        private static int GetY(HtmlNode node)
        {
            var yNode = node
                .Descendants("span")
                .FirstOrDefault(x => x.HasClass("coordinateY"));
            BrokenParserException.ThrowIfNull(yNode);
            return yNode.InnerText.ParseInt();
        }
    }
}