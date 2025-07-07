namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateBuildingCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        public sealed record Response(List<BuildingDto> Buildings, List<QueueBuilding> QueueBuildings);

        private static async ValueTask<Result<Response>> HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            IRxQueue rxQueue,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var (accountId, villageId) = command;

            var html = browser.Html;

            var dtoBuilding = GetBuildings(browser.CurrentUrl, html).ToList();
            if (dtoBuilding.Count == 0) return context.GetResponse(villageId);

            var dtoQueueBuilding = BuildingLayoutParser.GetQueueBuilding(html).ToList();

            var result = IsValidQueueBuilding(dtoQueueBuilding);
            if (result.IsFailed) return result;

            context.UpdateToDatabase(villageId, dtoBuilding, dtoQueueBuilding);

            rxQueue.Enqueue(new BuildingsModified(villageId));
            return context.GetResponse(villageId);
        }

        private static Response GetResponse(this AppDbContext context, VillageId villageId)
        {
            var buildings = context.Buildings
                .AsNoTracking()
                .Where(x => x.VillageId == villageId.Value)
                .ToDto()
                .ToList();
            var queueBuildings = context.QueueBuildings
                .AsNoTracking()
                .Where(x => x.VillageId == villageId.Value)
                .ToList();
            return new Response(buildings, queueBuildings);
        }

        private static IEnumerable<BuildingDto> GetBuildings(string url, HtmlDocument html)
        {
            if (url.Contains("dorf1"))
                return BuildingLayoutParser.GetFields(html);

            if (url.Contains("dorf2"))
                return BuildingLayoutParser.GetInfrastructures(html);

            return [];
        }

        private static Result IsValidQueueBuilding(List<QueueBuildingDto> dtos)
        {
            foreach (var strType in dtos.Select(x => x.Type))
            {
                var resultParse = Enum.TryParse(strType, false, out BuildingEnums _);
                if (!resultParse) return Stop.EnglishRequired(strType);
            }
            return Result.Ok();
        }

        private static void UpdateToDatabase(this AppDbContext context, VillageId villageId, List<BuildingDto> buildingDtos, List<QueueBuildingDto> queueBuildingDtos)
        {
            var dbBuildings = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .ToList();

            foreach (var dto in buildingDtos)
            {
                if (dto.Location == 40)
                {
                    var tribe = (TribeEnums)context.ByName(villageId, VillageSettingEnums.Tribe);
                    var wall = tribe.GetWall();
                    dto.Type = wall;
                }

                var dbBuilding = dbBuildings
                    .Find(x => x.Location == dto.Location);

                if (dbBuilding is null)
                {
                    var building = dto.ToEntity(villageId);
                    dbBuildings.Add(building);
                    context.Add(building);
                }
                else
                {
                    dto.To(dbBuilding);
                }
            }

            context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.CompleteTime < DateTime.Now)
                .ExecuteDelete();

            var dbQueueBuildings = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .ToList();

            var completeQueueBuildings = dbQueueBuildings
                .Where(x => x.CompleteTime < DateTime.Now)
                .OrderBy(x => x.Level)
                .ToList();

            if (completeQueueBuildings.Count > 0)
            {
                foreach (var completeQueueBuilding in completeQueueBuildings)
                {
                    if (completeQueueBuilding.Location == -1) continue;

                    var building = dbBuildings.Find(x => x.Location == completeQueueBuilding.Location);
                    if (building is null) continue;

                    building.Level = completeQueueBuilding.Level;
                    dbQueueBuildings.Remove(completeQueueBuilding);
                    context.Remove(completeQueueBuilding);
                }
            }

            var sets = new HashSet<string>();
            foreach (var dto in queueBuildingDtos)
            {
                var buildingType = Enum.Parse<BuildingEnums>(dto.Type);

                // same level same type but different location, bot need a way to check that
                if (sets.Add($"{dto.Level}-{dto.Type}"))
                {
                    var dbQueueBuilding = dbQueueBuildings.Find(x => x.Level == dto.Level && x.Type == buildingType);
                    if (dbQueueBuilding is null)
                    {
                        var building = dto.ToEntity(villageId);
                        dbQueueBuildings.Add(building);
                        context.Add(building);
                    }
                    else
                    {
                        dto.To(dbQueueBuilding);
                    }
                }
                else
                {
                    var dbDuplicateQueueBuildings = dbQueueBuildings
                        .FindAll(x => x.Level == dto.Level && x.Type == buildingType);

                    if (dbDuplicateQueueBuildings.Count > 1)
                    {
                        dto.To(dbDuplicateQueueBuildings[1]);
                    }
                    else
                    {
                        var building = dto.ToEntity(villageId);
                        dbQueueBuildings.Add(building);
                        context.Add(building);
                    }
                }

                var missingLocation = dbQueueBuildings
                    .Where(x => x.Location == -1)
                    .ToList();

                if (missingLocation.Count != 0)
                {
                    var underConstruction = dbBuildings
                        .Where(x => x.IsUnderConstruction)
                        .ToList();

                    for (var i = 0; i < missingLocation.Count; i++)
                    {
                        var queueBuilding = missingLocation[i];

                        var building = underConstruction.Find(x => x.Type == queueBuilding.Type && x.Level == queueBuilding.Level - 1);

                        if (building is null)
                        {
                            building = underConstruction.Find(x => x.Type == queueBuilding.Type && x.Level == queueBuilding.Level - 2);

                            if (building is null)
                            {
                                continue;
                            }
                        }
                        queueBuilding.Location = building.Location;
                        if (underConstruction.Where(x => x.Type == queueBuilding.Type).Count() > 1)
                        {
                            underConstruction.Remove(building);
                        }
                    }
                }
            }

            if (dbQueueBuildings.Count > queueBuildingDtos.Count)
            {
                for (var i = 0; i < dbQueueBuildings.Count; i++)
                {
                    var queueBuilding = dbQueueBuildings[i];
                    if (queueBuildingDtos.All(x => x.Level != queueBuilding.Level || Enum.Parse<BuildingEnums>(x.Type) != queueBuilding.Type))
                    {
                        context.Remove(queueBuilding);
                        var building = dbBuildings.Find(x => x.Location == queueBuilding.Location);
                        if (building is null) continue;

                        building.Level = queueBuilding.Level;
                    }
                }
            }

            context.SaveChanges();
        }
    }
}