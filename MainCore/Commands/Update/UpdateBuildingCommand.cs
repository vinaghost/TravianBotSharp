namespace MainCore.Commands.Update
{
    public class UpdateBuildingCommand(IDbContextFactory<AppDbContext> contextFactory = null, IMediator mediator = null)
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        private readonly IMediator _mediator = mediator ?? Locator.Current.GetService<IMediator>();

        public async Task<Result> Execute(IChromeBrowser chromeBrowser, AccountId accountId, VillageId villageId, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;

            var dtoBuilding = GetBuildings(chromeBrowser.CurrentUrl, html);
            if (!dtoBuilding.Any()) return Result.Ok();

            var dtoQueueBuilding = GetQueueBuilding(html);
            var queueBuildings = dtoQueueBuilding.ToList();
            var result = IsVaildQueueBuilding(queueBuildings);
            if (result.IsFailed) return result;

            UpdateQueueToDatabase(villageId, queueBuildings);
            UpdateBuildToDatabase(villageId, dtoBuilding.ToList());

            var dtoUnderConstructionBuildings = dtoBuilding.Where(x => x.IsUnderConstruction).ToList();
            UpdateQueueToDatabase(villageId, dtoUnderConstructionBuildings);
            await _mediator.Publish(new QueueBuildingUpdated(accountId, villageId), cancellationToken);

            return Result.Ok();
        }

        private static IEnumerable<BuildingDto> GetBuildings(string url, HtmlDocument html)
        {
            if (url.Contains("dorf1"))
                return GetFields(html);

            if (url.Contains("dorf2"))
                return GetInfrastructures(html);

            return [];
        }

        private static Result IsVaildQueueBuilding(List<QueueBuildingDto> dtos)
        {
            foreach (var strType in dtos.Select(x => x.Type))
            {
                var resultParse = Enum.TryParse(strType, false, out BuildingEnums _);
                if (!resultParse) return Stop.EnglishRequired(strType);
            }
            return Result.Ok();
        }

        private static IEnumerable<QueueBuildingDto> GetQueueBuilding(HtmlDocument doc)
        {
            static List<HtmlNode> GetNodes(HtmlDocument doc)
            {
                var finishButton = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("finishNow"));
                if (finishButton is null) return new();
                return finishButton.ParentNode.Descendants("li").ToList();
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

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var type = GetBuildingType(node);
                var level = GetLevel(node);
                var duration = GetDuration(node);
                yield return new()
                {
                    Position = i,
                    Type = type,
                    Level = level,
                    CompleteTime = DateTime.Now.Add(duration),
                    Location = -1,
                };
            }

            for (int i = nodes.Count; i < 4; i++) // we will save 3 slot for each village, Roman can build 3 building in one time
            {
                yield return new()
                {
                    Position = i,
                    Type = "Site",
                    Level = -1,
                    CompleteTime = DateTime.MaxValue,
                    Location = -1,
                };
            }
        }

        private static IEnumerable<BuildingDto> GetFields(HtmlDocument doc)
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
                var needClass = classess.FirstOrDefault(x => x.StartsWith("buildingSlot"));
                return needClass.ParseInt();
            }

            static BuildingEnums GetBuildingType(HtmlNode node)
            {
                var classess = node.GetClasses();
                var needClass = classess.FirstOrDefault(x => x.StartsWith("gid"));
                return (BuildingEnums)needClass.ParseInt();
            }

            static int GetLevel(HtmlNode node)
            {
                var classess = node.GetClasses();
                var needClass = classess.FirstOrDefault(x => x.StartsWith("level") && !x.Equals("level"));
                return needClass.ParseInt();
            }

            static bool IsUnderConstruction(HtmlNode node)
            {
                return node.GetClasses().Contains("underConstruction");
            }

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

        private static IEnumerable<BuildingDto> GetInfrastructures(HtmlDocument doc)
        {
            static List<HtmlNode> GetNodes(HtmlDocument doc)
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

        private void UpdateQueueToDatabase(VillageId villageId, List<BuildingDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();
            var queueBuildings = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .ToList();

            if (dtos.Count == 1)
            {
                var building = dtos[0];
                queueBuildings = queueBuildings
                    .Where(x => x.Type == building.Type)
                    .ToList();

                foreach (var item in queueBuildings)
                {
                    item.Location = building.Location;
                }
            }
            else if (dtos.Count == 2)
            {
                var typeCount = dtos.DistinctBy(x => x.Type).Count();

                if (typeCount == 2)
                {
                    foreach (var dto in dtos)
                    {
                        var queueBuilding = queueBuildings.Find(x => x.Type == dto.Type);
                        if (queueBuilding is not null)
                        {
                            queueBuilding.Location = dto.Location;
                        }
                    }
                }
                else if (typeCount == 1)
                {
                    queueBuildings = queueBuildings.Where(x => x.Type == dtos[0].Type).ToList();
                    if (dtos[0].Level == dtos[1].Level)
                    {
                        for (var i = 0; i < dtos.Count; i++)
                        {
                            queueBuildings[i].Location = dtos[i].Location;
                        }
                    }
                    else
                    {
                        foreach (var dto in dtos)
                        {
                            var queueBuilding = queueBuildings.Find(x => x.Level == dto.Level + 1);
                            if (queueBuilding is not null)
                            {
                                queueBuilding.Location = dto.Location;
                            }
                        }
                    }
                }
            }
            context.SaveChanges();
        }

        private void UpdateQueueToDatabase(VillageId villageId, List<QueueBuildingDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();

            context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .ExecuteDelete();

            var entities = new List<QueueBuilding>();

            foreach (var dto in dtos)
            {
                var queueBuilding = dto.ToEntity(villageId);
                entities.Add(queueBuilding);
            }

            context.AddRange(entities);
            context.SaveChanges();
        }

        private void UpdateBuildToDatabase(VillageId villageId, List<BuildingDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();
            var dbBuildings = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .ToList();

            foreach (var dto in dtos)
            {
                if (dto.Location == 40)
                {
                    var tribe = (TribeEnums)new GetSetting(_contextFactory).ByName(villageId, VillageSettingEnums.Tribe);
                    var wall = tribe.GetWall();
                    dto.Type = wall;
                }
                var dbBuilding = dbBuildings
                    .Find(x => x.Location == dto.Location);
                if (dbBuilding is null)
                {
                    var building = dto.ToEntity(villageId);
                    context.Add(building);
                }
                else
                {
                    dto.To(dbBuilding);
                    context.Update(dbBuilding);
                }
            }
            context.SaveChanges();
        }
    }
}