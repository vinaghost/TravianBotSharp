using Microsoft.Playwright;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;

namespace MainCore.Parsers
{
    public static partial class BuildingLayoutParser
    {
        public record struct RawFieldDto(string? SlotVal, string? GidVal, string? LevelVal, bool IsUnderConstruction);

        public static async Task<List<BuildingDto>> GetFields(IPage page)
        {
            var jsonResult = await page.EvaluateAsync<JsonElement>(@"() => {
                const elements = document.querySelectorAll('#resourceFieldContainer a.level');
                const result = [];

                const slotRegex = /buildingSlot(\d+)/;
                const gidRegex = /gid(\d+)/;
                const levelRegex = /\blevel(\d+)/;

                elements.forEach(field => {
                    const classAttr = field.getAttribute('class') || '';

                    const slotMatch = classAttr.match(slotRegex);
                    const gidMatch = classAttr.match(gidRegex);
                    const levelMatch = classAttr.match(levelRegex);
                    const isUnderConstruction = classAttr.includes('underConstruction');

                    result.push({
                        SlotVal: slotMatch ? slotMatch[1] : null,
                        GidVal: gidMatch ? gidMatch[1] : null,
                        LevelVal: levelMatch ? levelMatch[1] : null,
                        IsUnderConstruction: isUnderConstruction
                    });
                });
                return result;
            }");
            var text = jsonResult.GetRawText();
            var rawFieldsData = JsonSerializer.Deserialize<List<RawFieldDto>>(text) ?? throw new InvalidOperationException($"Failed to deserialize building data from the page. Content: {text}");

            var extractedData = new List<BuildingDto>();

            foreach (var raw in rawFieldsData)
            {
                extractedData.Add(new BuildingDto
                {
                    Location = int.TryParse(raw.SlotVal, out int slot) ? slot : -1,
                    Type = int.TryParse(raw.GidVal, out int gid) ? (BuildingEnums)gid : BuildingEnums.Unknown,
                    Level = int.TryParse(raw.LevelVal, out int level) ? level : -2,
                    IsUnderConstruction = raw.IsUnderConstruction
                });
            }

            return extractedData;
        }

        public record struct RawBuildingDto(int Index, string? LocStr, string? LevelStr, string? TypeStr, bool IsUnderConstruction);

        public static async Task<List<BuildingDto>> GetInfrastructures(IPage page)
        {
            var jsonResult = await page.EvaluateAsync<JsonElement>(@"() => {
                // Replace '.building-class' with the actual CSS selector for your buildings list
                const elements = document.querySelectorAll('#villageContent .buildingSlot');
                const result = [];

                elements.forEach((building, i) => {
                    if (i === 22) return;
                    const link = building.querySelector('a');

                    result.push({
                        Index: i,
                        LocStr: building.getAttribute('data-aid'),
                        LevelStr: link ? link.getAttribute('data-level') : null,
                        TypeStr: building.getAttribute('data-gid'),
                        IsUnderConstruction: link ? link.classList.contains('underConstruction') : false
                    });
                });
                return result;
            }");
            var text = jsonResult.GetRawText();
            var rawBuildingsData = JsonSerializer.Deserialize<List<RawBuildingDto>>(text) ?? throw new InvalidOperationException($"Failed to deserialize building data from the page. Content: {text}");

            var extractedData = new List<BuildingDto>();
            var tribe = await GetTribe(page);

            foreach (var raw in rawBuildingsData)
            {
                int location = int.TryParse(raw.LocStr, out int loc) ? loc : -1;
                int level = int.TryParse(raw.LevelStr, out int l) ? l : -1;

                var type = location switch
                {
                    26 => BuildingEnums.MainBuilding,
                    39 => BuildingEnums.RallyPoint,
                    40 => tribe.GetWall(),
                    _ => (BuildingEnums)(int.TryParse(raw.TypeStr, out int t) ? t : -1)
                };

                extractedData.Add(new BuildingDto
                {
                    Location = location,
                    Type = type,
                    Level = level,
                    IsUnderConstruction = raw.IsUnderConstruction
                });
            }

            return extractedData;
        }

        public record struct RawQueueBuildingDto(string? TypeName, string? LevelText, string? DurationText);

        public static async Task<List<QueueBuildingDto>> GetQueueBuilding(IPage page)
        {
            var jsonResult = await page.EvaluateAsync<JsonElement>(@"() => {
                const elements = document.querySelectorAll('.buildingList li');
                const result = [];

                elements.forEach(building => {
                    const nameEl = building.querySelector('.name');
                    const lvlEl = building.querySelector('.lvl');
                    const timerEl = building.querySelector('.timer');

                    // Extract the raw text from the first child node safely
                    const typeName = nameEl && nameEl.childNodes.length > 0
                        ? nameEl.childNodes[0].textContent.trim()
                        : '';

                    result.push({
                        TypeName: typeName,
                        LevelText: lvlEl ? lvlEl.innerText.trim() : '',
                        DurationText: timerEl ? (timerEl.getAttribute('value') || '') : ''
                    });
                });
                return result;
            }");

            var text = jsonResult.GetRawText();
            var rawQueueData = JsonSerializer.Deserialize<List<RawQueueBuildingDto>>(text) ?? throw new InvalidOperationException($"Failed to deserialize building data from the page. Content: {text}");
            var extractedData = new List<QueueBuildingDto>();

            foreach (var raw in rawQueueData)
            {
                string cleanedType = (raw.TypeName ?? "").Replace(" ", "");
                int level = int.TryParse(raw.LevelText, out int l) ? l : 0;
                int durationSeconds = int.TryParse(raw.DurationText, out int d) ? d : 0;

                extractedData.Add(new QueueBuildingDto
                {
                    Type = cleanedType,
                    Level = level,
                    CompleteTime = DateTime.Now.AddSeconds(durationSeconds),
                    Location = -1
                });
            }
            return extractedData;
        }

        public static async Task<TribeEnums> GetTribe(IPage page)
        {
            var tribeElement = page.Locator("#questmasterButton");
            var tribeSrc = await tribeElement.GetAttributeAsync("class");
            var tribeMatch = TribeExtractor().Match(tribeSrc ?? "");
            if (tribeMatch.Success)
            {
                int tribeId = int.Parse(tribeMatch.Groups[1].Value);
                return (TribeEnums)tribeId;
            }
            return TribeEnums.Any;
        }

        public static async Task<int> CountQueueBuilding(IPage page)
        {
            var buildings = page.Locator(".buildingList li");
            var buildingCount = await buildings.CountAsync();
            return buildingCount;
        }

        [GeneratedRegex(@"vid_(\d+)")]
        private static partial Regex TribeExtractor();
    }
}