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

        UpdateTribe(villageId, data.Tribe);
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

    private void UpdateTribe(VillageId villageId, int tribe)
    {
        using var context = contextFactory.CreateDbContext();
        var dict = new Dictionary<VillageSettingEnums, int>
        {
            { VillageSettingEnums.Tribe, tribe }
        };
        context.SaveVillageSetting(villageId, dict);
    }

    private void UpdateBuildingToDatabase(VillageId villageId, BuildingDto[] buildingDtos, QueueBuildingDto[] queueBuildingDtos)
    {
        using var context = contextFactory.CreateDbContext();
        var dbBuildings = context.Buildings.Where(x => x.VillageId == villageId.Value).ToList();
        var buildingsByLocation = dbBuildings.DistinctBy(x => x.Location).ToDictionary(x => x.Location);
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
                var newBuilding = new Building
                {
                    VillageId = villageId.Value,
                    Location = building.Location,
                    Type = (BuildingEnums)building.Type,
                    Level = building.Level,
                    IsUnderConstruction = building.IsUnderConstruction
                };
                context.Buildings.Add(newBuilding);
                dbBuildings.Add(newBuilding);
                buildingsByLocation.Add(newBuilding.Location, newBuilding);
            }
        }

        var now = DateTime.Now;
        var queueItem = context.QueueBuildings
            .Where(x => x.VillageId == villageId.Value)
            .ToList();

        var completeQueueBuildings = queueItem.Where(x => x.CompleteTime < now).ToList();
        context.RemoveRange(completeQueueBuildings);
        var notCompleteQueueBuildings = queueItem.Where(x => x.CompleteTime >= now).ToList();

        if (queueBuildingDtos.Length == 0)
        {
            context.RemoveRange(notCompleteQueueBuildings);
            context.SaveChanges();
            return;
        }

        var matchedQueueBuildings = new HashSet<QueueBuilding>();
        foreach (var dto in queueBuildingDtos)
        {
            // same level same type but different location, bot need a way to check that
            var queueBuilding = notCompleteQueueBuildings
                .Where(x => x.Level == dto.Level && x.Type == dto.Type && !matchedQueueBuildings.Contains(x))
                .OrderBy(x => x.CompleteTime)
                .FirstOrDefault();

            if (queueBuilding is null)
            {
                queueBuilding = new QueueBuilding
                {
                    VillageId = villageId.Value,
                    Type = dto.Type,
                    Level = dto.Level,
                    Location = -1
                };
                notCompleteQueueBuildings.Add(queueBuilding);
                context.Add(queueBuilding);
            }

            queueBuilding.Position = dto.Position;
            queueBuilding.CompleteTime = dto.CompleteTime;
            matchedQueueBuildings.Add(queueBuilding);
        }

        var staleQueueBuildings = notCompleteQueueBuildings.Where(x => !matchedQueueBuildings.Contains(x)).ToList();
        context.RemoveRange(staleQueueBuildings);
        notCompleteQueueBuildings.RemoveAll(x => !matchedQueueBuildings.Contains(x));

        var reservedSlots = notCompleteQueueBuildings
            .Where(x => x.Location != -1)
            .Select(x => (x.Location, x.Level))
            .ToHashSet();

        var missingLocation = notCompleteQueueBuildings
            .Where(x => x.Location == -1)
            .OrderBy(x => x.CompleteTime)
            .ToList();

        foreach (var queueBuilding in missingLocation)
        {
            var building = dbBuildings
                .Where(x => x.Type == queueBuilding.Type && x.Level == queueBuilding.Level - 1 && !reservedSlots.Contains((x.Location, queueBuilding.Level)))
                .OrderByDescending(x => x.IsUnderConstruction)
                .FirstOrDefault();

            // a further queued upgrade at the same location, chained behind the one in progress
            building ??= dbBuildings
                .Where(x => x.Type == queueBuilding.Type && x.IsUnderConstruction && x.Level < queueBuilding.Level - 1)
                .OrderByDescending(x => x.Level)
                .FirstOrDefault();

            if (building is null) continue;

            queueBuilding.Location = building.Location;
            reservedSlots.Add((building.Location, queueBuilding.Level));
        }

        context.SaveChanges();
    }
}