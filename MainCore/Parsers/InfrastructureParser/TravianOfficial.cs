namespace MainCore.Parsers.InfrastructureParser
{
    [RegisterAsParser]
    public class TravianOfficial : IInfrastructureParser
    {
        public IEnumerable<BuildingDto> Get(HtmlDocument doc)
        {
            var nodes = GetNodes(doc);
            foreach (var node in nodes)
            {
                var location = GetId(node);
                var level = GetLevel(node);
                var type = location switch
                {
                    26 => BuildingEnums.MainBuilding,
                    39 => BuildingEnums.RallyPoint,
                    _ => GetBuildingType(node)
                };
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
            var villageContentNode = doc.GetElementbyId("villageContent");
            if (villageContentNode is null) return new();
            var list = villageContentNode.Descendants("div").Where(x => x.HasClass("buildingSlot")).ToList();
            if (list.Count == 23) // level 1 wall and above has 2 part
            {
                list.RemoveAt(list.Count - 1);
            }

            return list;
        }

        private static int GetId(HtmlNode node)
        {
            return node.GetAttributeValue<int>("data-aid", -1);
        }

        private static BuildingEnums GetBuildingType(HtmlNode node)
        {
            return (BuildingEnums)node.GetAttributeValue<int>("data-gid", -1);
        }

        private static int GetLevel(HtmlNode node)
        {
            var aNode = node.Descendants("a").FirstOrDefault();
            if (aNode is null) return -1;
            return aNode.GetAttributeValue<int>("data-level", -1);
        }

        private static bool IsUnderConstruction(HtmlNode node)
        {
            return node.Descendants("a").Any(x => x.HasClass("underConstruction"));
        }
    }
}