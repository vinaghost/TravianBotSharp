namespace MainCore.Commands.Abstract
{
    public abstract class VillagePanelCommand
    {
        protected static HtmlNode GetVillageNode(HtmlDocument doc, VillageId villageId)
        {
            var villageBox = doc.GetElementbyId("sidebarBoxVillagelist");
            if (villageBox is null) return null;
            var villages = villageBox
                .Descendants("div")
                .Where(x => x.HasClass("listEntry"))
                .ToList();
            var village = villages.FirstOrDefault(x => GetId(x) == villageId);
            return village;
        }

        protected static VillageId GetCurrentVillageId(HtmlDocument doc)
        {
            var villageBox = doc.GetElementbyId("sidebarBoxVillagelist");
            if (villageBox is null) return VillageId.Empty;
            var village = villageBox
                .Descendants("div")
                .Where(x => x.HasClass("listEntry"))
                .Where(x => IsActive(x))
                .Select(x => GetId(x))
                .FirstOrDefault();
            return village;
        }

        protected static bool IsActive(HtmlNode node)
        {
            return node.HasClass("active");
        }

        private static VillageId GetId(HtmlNode node)
        {
            var dataDid = node.GetAttributeValue("data-did", 0);
            return new VillageId(dataDid);
        }

        protected static IEnumerable<VillageDto> Get(HtmlDocument doc)
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
            var villsNode = doc.GetElementbyId("sidebarBoxVillagelist");
            if (villsNode is null) return new();
            return villsNode.Descendants("div").Where(x => x.HasClass("listEntry")).ToList();
        }

        private static bool IsUnderAttack(HtmlNode node)
        {
            return node.HasClass("attack");
        }

        private static string GetName(HtmlNode node)
        {
            var textNode = node.Descendants("a").FirstOrDefault();
            if (textNode is null) return "";
            var nameNode = textNode.Descendants("span").FirstOrDefault(x => x.HasClass("name"));
            if (nameNode is null) return "";
            return nameNode.InnerText;
        }

        private static int GetX(HtmlNode node)
        {
            var xNode = node.Descendants("span").FirstOrDefault(x => x.HasClass("coordinateX"));
            if (xNode is null) return 0;
            return xNode.InnerText.ParseInt();
        }

        private static int GetY(HtmlNode node)
        {
            var yNode = node.Descendants("span").FirstOrDefault(x => x.HasClass("coordinateY"));
            if (yNode is null) return 0;
            return yNode.InnerText.ParseInt();
        }
    }
}