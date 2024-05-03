using HtmlAgilityPack;
using MainCore.DTO;
using System.Net;

namespace MainCore.Parsers.InfrastructureParser
{
    
    public class TTWars : IInfrastructureParser
    {
        public IEnumerable<BuildingDto> Get(HtmlDocument doc)
        {
            var nodes = GetNodes(doc);
            foreach (var node in nodes)
            {
                var location = GetId(node);
                var level = GetLevel(node);
                var type = GetBuildingType(node);
                var isUnderConstruction = IsUnderConstruction(node);

                yield return new BuildingDto()
                {
                    Location = location,
                    Level = level,
                    Type = type,
                    IsUnderConstruction = isUnderConstruction,
                };
            }
        }

        private static List<HtmlNode> GetNodes(HtmlDocument doc)
        {
            var villageContentNode = doc.GetElementbyId("village_map");
            if (villageContentNode is null) return new();
            var list = villageContentNode.Descendants("div").Where(x => x.HasClass("buildingSlot")).Skip(18).ToList();
            if (list.Count == 23) // level 1 wall and above has 2 part
            {
                list.RemoveAt(list.Count - 1);
            }

            return list;
        }

        private static int GetId(HtmlNode node)
        {
            var classess = node.GetClasses();
            var needClass = classess.FirstOrDefault(x => x.StartsWith("a"));
            if (string.IsNullOrEmpty(needClass)) return -1;
            var strResult = new string(needClass.Where(c => char.IsDigit(c)).ToArray());
            if (string.IsNullOrEmpty(strResult)) return -1;
            return int.Parse(strResult);
        }

        private static BuildingEnums GetBuildingType(HtmlNode node)
        {
            var classess = node.GetClasses();
            var needClass = classess.FirstOrDefault(x => x.StartsWith("g"));
            if (string.IsNullOrEmpty(needClass)) return BuildingEnums.Site;
            var strResult = new string(needClass.Where(c => char.IsDigit(c)).ToArray());
            if (string.IsNullOrEmpty(strResult)) return BuildingEnums.Site;

            return (BuildingEnums)int.Parse(strResult);
        }

        private static int GetLevel(HtmlNode node)
        {
            var labelLayerNode = node.Descendants("div").FirstOrDefault(x => x.HasClass("labelLayer"));
            if (labelLayerNode is null) return -1;
            var valueStrFixed = WebUtility.HtmlDecode(labelLayerNode.InnerText);
            if (string.IsNullOrEmpty(valueStrFixed)) return -1;
            var valueStr = new string(valueStrFixed.Where(c => char.IsDigit(c)).ToArray());
            if (string.IsNullOrEmpty(valueStr)) return -1;
            return int.Parse(valueStr);
        }

        private static bool IsUnderConstruction(HtmlNode node)
        {
            return node.Descendants("div").Any(x => x.HasClass("underConstruction"));
        }
    }
}