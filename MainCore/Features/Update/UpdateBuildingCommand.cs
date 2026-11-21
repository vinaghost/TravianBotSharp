using MainCore.Features.InstantUpgrade;
using MainCore.Features.Navigate;

namespace MainCore.Features.Update;

public class BuildingDto
{
    public int Type { get; set; }
    public int Level { get; set; }
    public bool IsUnderConstruction { get; set; }
    public int Location { get; set; }
}

public class QueueBuildingDto
{
    public string Name { get; set; } = "";
    public int Position { get; set; }
    public BuildingEnums Type { get; set; }
    public int Level { get; set; }
    public int DurationSeconds { get; set; }
    public DateTime CompleteTime { get; set; }
}

[Handler]
public sealed partial class UpdateBuildingCommand(
    IDbContextFactory<AppDbContext> contextFactory,
    TaskManager taskManager,
    RxQueue rxQueue)
{
    public sealed record Command(IChromeBrowser Browser, VillageId VillageId) : IVillageBrowserCommand
    {
        public string Name => "";
    }

    private async ValueTask HandleAsync(Command command)
    {
        var (browser, accountId, villageId) = command;
        var data = await browser.CurrentPage.GetBuildingData(browser.BuildingNames);

        UpdateBuildingToDatabase(villageId, data.Buildings, data.Queue);
        TriggerTask(accountId, villageId);
        rxQueue.Enqueue(new BuildingsModified(accountId, villageId));
    }

    private void TriggerTask(AccountId accountId, VillageId villageId)
    {
        if (taskManager.IsExist<InstantUpgradeTask.Task>(accountId, villageId)) return;
        using var context = contextFactory.CreateDbContext();
        if (!context.IsInstantUpgradeEnable(accountId, villageId)) return;

        taskManager.Add(new InstantUpgradeTask.Task(accountId, villageId), true);
    }

    private void UpdateBuildingToDatabase(VillageId villageId, BuildingDto[] buildingDtos, QueueBuildingDto[] queueBuildingDtos)
    {
        using var context = contextFactory.CreateDbContext();
        var dbBuildings = context.Buildings.Where(x => x.VillageId == villageId.Value).ToList();
        var buildingsByLocation = dbBuildings.ToDictionary(x => x.Location);
        foreach (var building in buildingDtos)
        {
            if (buildingsByLocation.TryGetValue(building.Location, out var dbBuilding))
            {
                dbBuilding.Type = (BuildingEnums)building.Type;
                dbBuilding.Level = building.Level;
                dbBuilding.IsUnderConstruction = building.IsUnderConstruction;
            }
            else
            {
                context.Buildings.Add(new Building
                {
                    VillageId = villageId.Value,
                    Location = building.Location,
                    Type = (BuildingEnums)building.Type,
                    Level = building.Level,
                    IsUnderConstruction = building.IsUnderConstruction
                });
            }
        }

        var queueItem = context.QueueBuildings
            .Where(x => x.VillageId == villageId.Value)
            .ToList();

        var completeQueueBuildings = queueItem.Where(x => x.CompleteTime < DateTime.Now);
        context.RemoveRange(completeQueueBuildings);
        var notCompleteQueueBuildings = queueItem.Where(x => x.CompleteTime >= DateTime.Now).ToList();

        if (queueBuildingDtos.Length == 0)
        {
            context.RemoveRange(notCompleteQueueBuildings);
            context.SaveChanges();
            return;
        }

        var sets = new HashSet<string>();
        foreach (var dto in queueBuildingDtos)
        {
            // same level same type but different location, bot need a way to check that
            if (sets.Add($"{dto.Name}+{dto.Level}"))
            {
                var existQueueBuilding = notCompleteQueueBuildings
                    .OrderBy(x => x.CompleteTime)
                    .FirstOrDefault(x => x.Level == dto.Level && x.Type == dto.Type);
                if (existQueueBuilding is null)
                {
                    var queueBuilding = new QueueBuilding
                    {
                        VillageId = villageId.Value,
                        Position = dto.Position,
                        Type = dto.Type,
                        Level = dto.Level,
                        Location = -1,
                        CompleteTime = dto.CompleteTime
                    };
                    notCompleteQueueBuildings.Add(queueBuilding);
                    context.Add(queueBuilding);
                }
                else
                {
                    existQueueBuilding.Position = dto.Position;
                    existQueueBuilding.CompleteTime = dto.CompleteTime;
                }
            }
            else
            {
                var existQueueBuildings = notCompleteQueueBuildings
                    .Where(x => x.Level == dto.Level && x.Type == dto.Type)
                    .OrderBy(x => x.CompleteTime)
                    .ToList();

                if (existQueueBuildings.Count == 2)
                {
                    existQueueBuildings[1].Position = dto.Position;
                    existQueueBuildings[1].CompleteTime = dto.CompleteTime;
                }
                else
                {
                    var queueBuilding = new QueueBuilding
                    {
                        VillageId = villageId.Value,
                        Position = dto.Position,
                        Type = dto.Type,
                        Level = dto.Level,
                        Location = -1,
                        CompleteTime = dto.CompleteTime
                    };

                    notCompleteQueueBuildings.Add(queueBuilding);
                    context.Add(queueBuilding);
                }
            }
        }

        var missingLocation = notCompleteQueueBuildings
            .Where(x => x.Location == -1)
            .OrderBy(x => x.CompleteTime)
            .ToList();

        if (missingLocation.Count > 0)
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
                    building = underConstruction.Find(x => x.Type == queueBuilding.Type);

                    if (building is null)
                    {
                        continue;
                    }
                }

                queueBuilding.Location = building.Location;
                if (underConstruction.Count(x => x.Type == queueBuilding.Type) > 1)
                {
                    underConstruction.Remove(building);
                }
            }
        }

        context.SaveChanges();
    }
}