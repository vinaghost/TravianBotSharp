namespace MainCore.Parsers
{
    public static class BuildingLayoutParser
    {
        public static IEnumerable<BuildingDto> GetFields(HtmlDocument doc)
        {
            static IEnumerable<HtmlNode> GetNodes(HtmlDocument doc)
            {
                var resourceFieldContainerNode = doc.GetElementbyId("resourceFieldContainer");
                if (resourceFieldContainerNode is null) return [];

                var nodes = resourceFieldContainerNode
                    .ChildNodes
                    .Where(x => x.HasClass("level"));
                return nodes;
            }

            static int GetId(HtmlNode node)
            {
                var classess = node.GetClasses();
                var buildingSlot = classess.FirstOrDefault(x => x.StartsWith("buildingSlot"));
                if (buildingSlot is null) return -1;
                return buildingSlot.ParseInt();
            }

            static BuildingEnums GetBuildingType(HtmlNode node)
            {
                var classess = node.GetClasses();
                var gid = classess.FirstOrDefault(x => x.StartsWith("gid"));
                if (gid is null) return BuildingEnums.Unknown;
                return (BuildingEnums)gid.ParseInt();
            }

            static int GetLevel(HtmlNode node)
            {
                var classess = node.GetClasses();
                var level = classess.FirstOrDefault(x => x.StartsWith("level") && !x.Equals("level"));
                if (level is null) return -1;
                return level.ParseInt();
            }

            static bool IsUnderConstruction(HtmlNode node)
            {
                return node.GetClasses().Contains("underConstruction");
            }

            foreach (var node in GetNodes(doc))
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

        public static IEnumerable<BuildingDto> GetInfrastructures(HtmlDocument doc)
        {
            static IEnumerable<HtmlNode> GetNodes(HtmlDocument doc)
            {
                var villageContentNode = doc.GetElementbyId("villageContent");
                if (villageContentNode is null) return [];
                var list = villageContentNode.Descendants("div").Where(x => x.HasClass("buildingSlot"));
                if (list.Count() == 23) // level 1 wall and above has 2 part
                {
                    return list.SkipLast(1);
                }

                return list;
            }

            static int GetId(HtmlNode node)
            {
                return node.GetAttributeValue<int>("data-aid", -1);
            }

            static BuildingEnums GetBuildingType(HtmlNode node)
            {
                return (BuildingEnums)node.GetAttributeValue<int>("data-gid", -1);
            }

            static int GetLevel(HtmlNode node)
            {
                var aNode = node.Descendants("a").FirstOrDefault();
                if (aNode is null) return -1;
                return aNode.GetAttributeValue<int>("data-level", -1);
            }

            static bool IsUnderConstruction(HtmlNode node)
            {
                return node.Descendants("a").Any(x => x.HasClass("underConstruction"));
            }

            foreach (var node in GetNodes(doc))
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

        public static IEnumerable<QueueBuildingDto> GetQueueBuilding(HtmlDocument doc)
        {
            static IEnumerable<HtmlNode> GetNodes(HtmlDocument doc)
            {
                var finishButton = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("finishNow"));
                if (finishButton is null) return [];
                return finishButton.ParentNode.Descendants("li");
            }

            static string GetBuildingType(HtmlNode node)
            {
                var nodeName = node.Descendants("div").FirstOrDefault(x => x.HasClass("name"));
                if (nodeName is null) return "";

                return new string(nodeName.ChildNodes[0].InnerText.Where(c => char.IsLetter(c) || char.IsDigit(c)).ToArray());
            }

            static int GetLevel(HtmlNode node)
            {
                var nodeLevel = node.Descendants("span").FirstOrDefault(x => x.HasClass("lvl"));
                if (nodeLevel is null) return 0;

                return nodeLevel.InnerText.ParseInt();
            }

            static TimeSpan GetDuration(HtmlNode node)
            {
                var nodeTimer = node.Descendants().FirstOrDefault(x => x.HasClass("timer"));
                if (nodeTimer is null) return TimeSpan.Zero;
                int sec = nodeTimer.GetAttributeValue("value", 0);
                return TimeSpan.FromSeconds(sec);
            }

            var nodes = GetNodes(doc);
            foreach (var node in nodes)
            {
                var type = GetBuildingType(node);
                var level = GetLevel(node);
                var duration = GetDuration(node);
                yield return new QueueBuildingDto()
                {
                    Type = type,
                    Level = level,
                    CompleteTime = DateTime.Now.Add(duration),
                    Location = -1,
                };
            }
        }
    }
}
