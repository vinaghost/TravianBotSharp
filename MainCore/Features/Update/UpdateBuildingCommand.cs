using System.Text.Json;
using System.Text.RegularExpressions;

using MainCore.Features.InstantUpgrade;

using Microsoft.EntityFrameworkCore.Internal;

namespace MainCore.Features.Update;

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

    private async ValueTask<Result> HandleAsync(Command command)
    {
        var (browser, accountId, villageId) = command;

        var (dtoBuilding, dtoQueueBuilding, tribe) = await browser.CurrentPage.GetBuildingData();
        UpdateTribe(villageId, tribe);

        var result = IsValidQueueBuilding(dtoQueueBuilding);
        if (result.IsFailed) return result;

        if (dtoBuilding.Count == 0) return Result.Ok();
        UpdateBuildings(villageId, dtoBuilding, dtoQueueBuilding);

        TriggerTask(accountId, villageId);

        rxQueue.Enqueue(new BuildingsModified(accountId, villageId));

        return Result.Ok();
    }

    private void TriggerTask(AccountId accountId, VillageId villageId)
    {
        using var context = contextFactory.CreateDbContext();
        if (!taskManager.IsExist<InstantUpgradeTask.Task>(accountId, villageId)) return;
        if (!context.IsInstantUpgradeEnable(accountId, villageId)) return;

        taskManager.Add(new InstantUpgradeTask.Task(accountId, villageId), true);
    }

    private static Result IsValidQueueBuilding(List<QueueBuildingDto> dtos)
    {
        foreach (var strType in dtos.Select(x => x.Type))
        {
            var resultParse = Enum.TryParse(strType, false, out BuildingEnums _);
            if (!resultParse) return Stop.Error.WithError($"Cannot parse {strType}. Is language English ?");
        }
        return Result.Ok();
    }

    private void UpdateTribe(VillageId villageId, TribeEnums tribe)
    {
        using var context = contextFactory.CreateDbContext();
        context.VillagesSetting
            .Where(x => x.VillageId == villageId.Value)
            .Where(x => x.Setting == VillageSettingEnums.Tribe)
            .ExecuteUpdate(x => x.SetProperty(x => x.Value, (int)tribe));
    }

    private void UpdateBuildings(VillageId villageId, List<BuildingDto> buildingDtos, List<QueueBuildingDto> queueBuildingDtos)
    {
        using var context = contextFactory.CreateDbContext();
        var now = DateTime.Now;

        bool updatingDorf1 = buildingDtos.Any(b => b.Type.IsResourceField());

        var dbBuildings = context.Buildings
            .Where(x => x.VillageId == villageId.Value)
            .ToList();

        var buildingMap = dbBuildings.ToDictionary(x => x.Location);

        foreach (var dto in buildingDtos)
        {
            if (!buildingMap.TryGetValue(dto.Location, out var dbBuilding))
            {
                var building = new Building
                {
                    VillageId = villageId.Value,
                    Type = dto.Type,
                    Level = dto.Level,
                    IsUnderConstruction = dto.IsUnderConstruction,
                    Location = dto.Location
                };
                dbBuildings.Add(building);
                buildingMap[dto.Location] = building;
                context.Buildings.Add(building);
            }
            else
            {
                dbBuilding.Type = dto.Type;
                dbBuilding.Level = dto.Level;
                dbBuilding.IsUnderConstruction = dto.IsUnderConstruction;
                dbBuilding.Location = dto.Location;
            }
        }

        var dbQueueBuildings = context.QueueBuildings
            .Where(x => x.VillageId == villageId.Value)
            .ToList();

        var completeQueueBuildings = dbQueueBuildings
            .Where(x => x.CompleteTime < now && x.Location != -1)
            .OrderBy(x => x.Level)
            .ToList();

        foreach (var completeQueueBuilding in completeQueueBuildings)
        {
            if (buildingMap.TryGetValue(completeQueueBuilding.Location, out var building))
            {
                building.Level = completeQueueBuilding.Level;
                dbQueueBuildings.Remove(completeQueueBuilding);
                context.Remove(completeQueueBuilding);
            }
        }

        var dbQueueGroups = dbQueueBuildings
            .GroupBy(x => new { x.Level, x.Type })
            .ToDictionary(g => g.Key, g => g.ToList());

        var matchedDbQueueItems = new HashSet<QueueBuilding>();

        foreach (var dto in queueBuildingDtos)
        {
            var buildingType = Enum.Parse<BuildingEnums>(dto.Type);
            var key = new { dto.Level, Type = buildingType };

            if (dbQueueGroups.TryGetValue(key, out var existingDbItems) && existingDbItems.Count > 0)
            {
                var match = existingDbItems.FirstOrDefault(x => x.Location != -1) ?? existingDbItems[0];
                matchedDbQueueItems.Add(match);
                existingDbItems.Remove(match);
            }
            else
            {
                var newQueueBuilding = new QueueBuilding
                {
                    VillageId = villageId.Value,
                    Position = dto.Position,
                    Location = dto.Location,
                    Type = buildingType,
                    Level = dto.Level,
                    CompleteTime = dto.CompleteTime
                };

                dbQueueBuildings.Add(newQueueBuilding);
                context.Add(newQueueBuilding);

                if (!dbQueueGroups.TryGetValue(key, out var list))
                {
                    list = [];
                    dbQueueGroups[key] = list;
                }
                list.Add(newQueueBuilding);
            }
        }

        var missingLocation = dbQueueBuildings.Where(x => x.Location == -1).ToList();
        if (missingLocation.Count > 0)
        {
            var underConstruction = dbBuildings.Where(x => x.IsUnderConstruction).ToList();

            foreach (var queueBuilding in missingLocation)
            {
                bool isResource = queueBuilding.Type.IsResourceField();
                if ((updatingDorf1 && !isResource) || (!updatingDorf1 && isResource))
                {
                    continue;
                }

                var building = underConstruction.Find(x => x.Type == queueBuilding.Type && x.Level == queueBuilding.Level - 1)
                            ?? underConstruction.Find(x => x.Type == queueBuilding.Type && x.Level == queueBuilding.Level - 2);

                if (building is null) continue;

                queueBuilding.Location = building.Location;

                if (underConstruction.Count(x => x.Type == queueBuilding.Type) > 1)
                {
                    underConstruction.Remove(building);
                }
            }
        }

        foreach (var dbQueueItem in dbQueueBuildings.ToList())
        {
            if (matchedDbQueueItems.Contains(dbQueueItem)) continue;
            bool isResource = dbQueueItem.Type.IsResourceField();

            if ((updatingDorf1 && isResource) || (!updatingDorf1 && !isResource))
            {
                context.Remove(dbQueueItem);
                dbQueueBuildings.Remove(dbQueueItem);

                if (buildingMap.TryGetValue(dbQueueItem.Location, out var building))
                {
                    building.Level = dbQueueItem.Level;
                }
            }
        }

        context.SaveChanges();
    }
}