using System.Text.Json;
using System.Text.RegularExpressions;

namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateBuildingCommand(
        ChromeManager chromeManager,
        IDbContextFactory<AppDbContext> contextFactory,
        RxQueue rxQueue)
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand
        {
            public string Name => "";
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var (accountId, villageId) = command;

            var browser = chromeManager.Get(accountId);

            var (dtoBuilding, dtoQueueBuilding, tribe) = await GetData(browser.CurrentPage);
            UpdateTribe(villageId, tribe);

            var result = IsValidQueueBuilding(dtoQueueBuilding);
            if (result.IsFailed) return result;

            if (dtoBuilding.Count == 0) return Result.Ok();
            UpdateBuildings(villageId, dtoBuilding, dtoQueueBuilding);

            rxQueue.Enqueue(new BuildingsModified(accountId, villageId));

            return Result.Ok();
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

        public record struct RawBuildingDto(string? LocStr, string? TypeStr, string? LevelStr, bool IsUnderConstruction);
        public record struct RawQueueBuildingDto(string? TypeName, string? LevelText, string? DurationText);
        public record struct CombinedPageDataDto(
            List<RawBuildingDto> Buildings,
            List<RawQueueBuildingDto> Queue,
            string TribeClass,
            bool IsDorf1
        );

        [GeneratedRegex(@"vid_(\d+)")]
        private static partial Regex TribeExtractor();

        [GeneratedRegex(@"\d+")]
        private static partial Regex LevelExtractor();

        public static async Task<(List<BuildingDto> Buildings, List<QueueBuildingDto> Queue, TribeEnums Tribe)> GetData(IPage page)
        {
            var jsonResult = await page.EvaluateAsync<JsonElement>(@"() => {
                const url = window.location.href;
                const isDorf1 = url.includes('dorf1');
                const buildingsResult = [];
                const queueResult = [];

                // --- 1. PARSE BUILDINGS (Dorf1 vs Dorf2 dependent) ---
                if (isDorf1) {
                    const elements = document.querySelectorAll('#resourceFieldContainer a.level');
                    const locRegex = /buildingSlot(\d+)/;
                    const gidRegex = /gid(\d+)/;
                    const levelRegex = /\blevel(\d+)/;

                    elements.forEach(field => {
                        const classAttr = field.getAttribute('class') || '';
                        const locMatch = classAttr.match(slotRegex);
                        const gidMatch = classAttr.match(gidRegex);
                        const levelMatch = classAttr.match(levelRegex);
                        const isUnderConstruction = classAttr.includes('underConstruction');

                        buildingsResult.push({
                            LocStr: locMatch ? locMatch[1] : null,
                            TypeStr: gidMatch ? gidMatch[1] : null,
                            LevelStr: levelMatch ? levelMatch[1] : null,
                            IsUnderConstruction: isUnderConstruction
                        });
                    });
                } else {
                    const elements = document.querySelectorAll('#villageContent .buildingSlot');
                    elements.forEach((building, i) => {
                        if (i === 22) return;
                        const link = building.querySelector('a');

                        buildingsResult.push({
                            LocStr: building.getAttribute('data-aid'),
                            TypeStr: building.getAttribute('data-gid'),
                            LevelStr: link ? link.getAttribute('data-level') : null,
                            IsUnderConstruction: link ? link.classList.contains('underConstruction') : false
                        });
                    });
                }

                // --- 2. PARSE QUEUE BUILDINGS (Always visible on both pages) ---
                const queueElements = document.querySelectorAll('.buildingList li');
                queueElements.forEach(building => {
                    const nameEl = building.querySelector('.name');
                    const lvlEl = building.querySelector('.lvl');
                    const timerEl = building.querySelector('.timer');

                    const typeName = nameEl && nameEl.childNodes.length > 0
                        ? nameEl.childNodes[0].textContent.trim()
                        : '';

                    queueResult.push({
                        TypeName: typeName,
                        LevelText: lvlEl ? lvlEl.innerText.trim() : '',
                        DurationText: timerEl ? (timerEl.getAttribute('value') || '') : ''
                    });
                });

                // --- 3. PARSE TRIBE CONTEXT ---
                const questMaster = document.querySelector('#questmasterButton');
                const tribeClass = questMaster ? (questMaster.getAttribute('class') || '') : '';

                return {
                    Buildings: buildingsResult,
                    Queue: queueResult,
                    TribeClass: tribeClass,
                    IsDorf1: isDorf1
                };
            }");

            var text = jsonResult.GetRawText();
            var rawData = JsonSerializer.Deserialize<CombinedPageDataDto>(text);

            var tribeMatch = TribeExtractor().Match(rawData.TribeClass);
            TribeEnums tribe = tribeMatch.Success ? (TribeEnums)int.Parse(tribeMatch.Groups[1].Value) : TribeEnums.Any;

            // --- Process Queue items ---
            var queueData = new List<QueueBuildingDto>();
            foreach (var raw in rawData.Queue)
            {
                string cleanedType = new(raw.TypeName?.Where(c => char.IsLetter(c) || char.IsDigit(c)).ToArray() ?? []);
                var levelMatch = LevelExtractor().Match(raw.LevelText ?? "");
                int level = levelMatch.Success && int.TryParse(levelMatch.Value, out var l) ? l : 0;
                int durationSeconds = int.TryParse(raw.DurationText, out int d) ? d : 0;

                queueData.Add(new QueueBuildingDto
                {
                    Type = cleanedType,
                    Level = level,
                    CompleteTime = DateTime.Now.AddSeconds(durationSeconds),
                    Location = -1
                });
            }

            // --- Process Buildings List ---
            var buildingsData = new List<BuildingDto>();
            foreach (var raw in rawData.Buildings)
            {
                int location = int.TryParse(raw.LocStr, out int loc) ? loc : -1;

                BuildingEnums type;
                int level;

                if (rawData.IsDorf1)
                {
                    type = int.TryParse(raw.TypeStr, out int gid) ? (BuildingEnums)gid : BuildingEnums.Unknown;
                    level = int.TryParse(raw.LevelStr, out int lvl) ? lvl : -1;
                }
                else
                {
                    level = int.TryParse(raw.LevelStr, out int l) ? l : -1;
                    type = location switch
                    {
                        26 => BuildingEnums.MainBuilding,
                        39 => BuildingEnums.RallyPoint,
                        40 => tribe.GetWall(),
                        _ => (BuildingEnums)(int.TryParse(raw.TypeStr, out int t) ? t : -1)
                    };
                }

                buildingsData.Add(new BuildingDto
                {
                    Location = location,
                    Type = type,
                    Level = level,
                    IsUnderConstruction = raw.IsUnderConstruction
                });
            }

            return (buildingsData, queueData, tribe);
        }
    }
}