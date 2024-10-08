using MainCore.Commands.Abstract;

namespace MainCore.Commands.Update
{
    [RegisterScoped<UpdateBuildingCommand>]
    public class UpdateBuildingCommand(DataService dataService, IDbContextFactory<AppDbContext> contextFactory, IMediator mediator) : CommandBase(dataService), ICommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;
        private readonly IMediator _mediator = mediator;

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var accountId = _dataService.AccountId;
            var villageId = _dataService.VillageId;
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;

            var dtoBuilding = GetBuildings(chromeBrowser.CurrentUrl, html);
            if (!dtoBuilding.Any()) return Result.Ok();

            var dtoQueueBuilding = BuildingLayoutParser.GetQueueBuilding(html);
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