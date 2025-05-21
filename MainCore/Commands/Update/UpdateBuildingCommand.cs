using MainCore.Constraints;
using MainCore.Notifications.Behaviors;

namespace MainCore.Commands.Update
{
    [Handler]
    [Behaviors(typeof(BuildingUpdatedBehavior<,>))]
    public static partial class UpdateBuildingCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        public sealed record Response(List<BuildingDto> Buildings, List<QueueBuilding> QueueBuildings);

        private static async ValueTask<Result<Response>> HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var (accountId, villageId) = command;
            context.Clean(villageId);

            var html = browser.Html;

            var dtoBuilding = GetBuildings(browser.CurrentUrl, html);
            if (!dtoBuilding.Any()) return Result.Ok();

            var dtoQueueBuilding = BuildingLayoutParser.GetQueueBuilding(html);
            var queueBuildings = dtoQueueBuilding.ToList();
            var result = IsVaildQueueBuilding(queueBuildings);
            if (result.IsFailed) return result;

            context.UpdateQueueToDatabase(villageId, queueBuildings);
            context.UpdateBuildToDatabase(villageId, dtoBuilding.ToList());

            var dtoUnderConstructionBuildings = dtoBuilding.Where(x => x.IsUnderConstruction).ToList();
            context.UpdateQueueToDatabase(villageId, dtoUnderConstructionBuildings);

            context.SaveChanges();

            return context.GetResponse(villageId);
        }

        private static Response GetResponse(this AppDbContext context, VillageId villageId)
        {
            var buildings = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .ToDto()
                .ToList();
            var queueBuildings = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
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

        private static Result IsVaildQueueBuilding(List<QueueBuildingDto> dtos)
        {
            foreach (var strType in dtos.Select(x => x.Type))
            {
                var resultParse = Enum.TryParse(strType, false, out BuildingEnums _);
                if (!resultParse) return Stop.EnglishRequired(strType);
            }
            return Result.Ok();
        }

        private static void UpdateQueueToDatabase(this AppDbContext context, VillageId villageId, List<BuildingDto> dtos)
        {
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
            context.UpdateRange(queueBuildings);
        }

        private static void UpdateQueueToDatabase(this AppDbContext context, VillageId villageId, List<QueueBuildingDto> dtos)
        {
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
        }

        private static void UpdateBuildToDatabase(this AppDbContext context, VillageId villageId, List<BuildingDto> dtos)
        {
            var dbBuildings = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .ToList();

            foreach (var dto in dtos)
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
                    context.Add(building);
                }
                else
                {
                    dto.To(dbBuilding);
                    context.Update(dbBuilding);
                }
            }
        }

        private static void Clean(this AppDbContext context, VillageId villageId)
        {
            var now = DateTime.Now;
            var completeBuildingQuery = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .Where(x => x.CompleteTime < now);

            var completeBuildingLocations = completeBuildingQuery
                .Select(x => x.Location)
                .ToList();

            foreach (var completeBuildingLocation in completeBuildingLocations)
            {
                context.Buildings
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Location == completeBuildingLocation)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Level, x => x.Level + 1));
            }

            completeBuildingQuery
                .ExecuteUpdate(x => x.SetProperty(x => x.Type, BuildingEnums.Site));

            context.ChangeTracker.Clear();
        }
    }
}