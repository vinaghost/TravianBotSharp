using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.DTO;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.FieldParser
{
    [RegisterAsTransient(ServerEnums.TravianOfficial)]
    public class TravianOfficial : IFieldParser
    {
        public bool IsUnderAttack(HtmlDocument doc)
        {
            var villageInfobox = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("villageInfobox"));
            if (villageInfobox is null) return false;

            return villageInfobox
                .Descendants("img")
                .Any(x => x.HasClass("att1") || x.HasClass("att3"));
        }

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

        private static IEnumerable<HtmlNode> GetNodes(HtmlDocument doc)
        {
            var resourceFieldContainerNode = doc.GetElementbyId("resourceFieldContainer");
            if (resourceFieldContainerNode is null) return Enumerable.Empty<HtmlNode>();

            var nodes = resourceFieldContainerNode
                .ChildNodes
                .Where(x => x.HasClass("level"));
            return nodes;
        }

        private static int GetId(HtmlNode node)
        {
            var classess = node.GetClasses();
            var needClass = classess.FirstOrDefault(x => x.StartsWith("buildingSlot"));
            if (string.IsNullOrEmpty(needClass)) return -1;
            var strResult = new string(needClass.Where(c => char.IsDigit(c)).ToArray());
            if (string.IsNullOrEmpty(strResult)) return -1;

            return int.Parse(strResult);
        }

        private static BuildingEnums GetBuildingType(HtmlNode node)
        {
            var classess = node.GetClasses();
            var needClass = classess.FirstOrDefault(x => x.StartsWith("gid"));
            if (string.IsNullOrEmpty(needClass)) return BuildingEnums.Site;
            var strResult = new string(needClass.Where(c => char.IsDigit(c)).ToArray());
            if (string.IsNullOrEmpty(strResult)) return BuildingEnums.Site;

            return (BuildingEnums)int.Parse(strResult);
        }

        private static int GetLevel(HtmlNode node)
        {
            var classess = node.GetClasses();
            var needClass = classess.FirstOrDefault(x => x.StartsWith("level") && !x.Equals("level"));
            if (string.IsNullOrEmpty(needClass)) return -1;
            var strResult = new string(needClass.Where(c => char.IsDigit(c)).ToArray());
            if (string.IsNullOrEmpty(strResult)) return -1;

            return int.Parse(strResult);
        }

        private static bool IsUnderConstruction(HtmlNode node)
        {
            return node.GetClasses().Contains("underConstruction");
        }
    }
}