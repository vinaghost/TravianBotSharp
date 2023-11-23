using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.VillagePanelParser
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : IVillagePanelParser
    {
        public HtmlNode GetVillageNode(HtmlDocument doc, VillageId villageId)
        {
            var villageBox = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.Id.Equals("sidebarBoxVillagelist"));
            if (villageBox is null) return null;
            var villages = villageBox
                            .Descendants("li")
                            .ToList();
            var village = villages.FirstOrDefault(x => GetId(x) == villageId);
            return village;
        }

        public bool IsActive(HtmlNode node)
        {
            return node.HasClass("active");
        }

        public IEnumerable<VillageDto> Get(HtmlDocument doc)
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
            var villsNode = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.Id.Equals("sidebarBoxVillagelist"));
            if (villsNode is null) return null;
            return villsNode.Descendants("li").ToList();
        }

        private static bool IsUnderAttack(HtmlNode node)
        {
            return node.HasClass("attack");
        }

        private static VillageId GetId(HtmlNode node)
        {
            var hrefNode = node.ChildNodes.FirstOrDefault(x => x.Name == "a");
            if (hrefNode is null) return VillageId.Empty;
            var href = System.Net.WebUtility.HtmlDecode(hrefNode.GetAttributeValue("href", ""));
            if (string.IsNullOrEmpty(href)) return VillageId.Empty;
            if (!href.Contains('=')) return VillageId.Empty;
            var value = href.Split('=')[1];
            if (value.Contains('&'))
            {
                value = value.Split('&')[0];
            }
            return new VillageId(int.Parse(value));
        }

        private static string GetName(HtmlNode node)
        {
            var textNode = node.Descendants("span").FirstOrDefault(x => x.HasClass("name"));
            if (textNode is null) return "";
            return textNode.InnerText;
        }

        private static int GetX(HtmlNode node)
        {
            var xNode = node.Descendants("span").FirstOrDefault(x => x.HasClass("coordinateX"));
            if (xNode is null) return 0;
            var xStr = new string(xNode.InnerText.Where(c => char.IsDigit(c) || c.Equals('-')).ToArray());
            if (string.IsNullOrEmpty(xStr)) return 0;
            return int.Parse(xStr);
        }

        private static int GetY(HtmlNode node)
        {
            var yNode = node.Descendants("span").FirstOrDefault(x => x.HasClass("coordinateY"));
            if (yNode is null) return 0;
            var yStr = new string(yNode.InnerText.Where(c => char.IsDigit(c) || c.Equals('-')).ToArray());
            if (string.IsNullOrEmpty(yStr)) return 0;

            return int.Parse(yStr);
        }
    }
}