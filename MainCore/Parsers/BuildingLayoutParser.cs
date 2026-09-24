using Microsoft.Playwright;
using System.Text.RegularExpressions;
using System.Xml;

namespace MainCore.Parsers
{
    public static partial class BuildingLayoutParser
    {
        public static ILocator GetBuilding(IPage page, int location)
        {
            if (location < 19) return GetField(page, location);
            return GetInfrastructure(page, location);
        }

        private static ILocator GetField(IPage page, int location)
        {
            var node = page.Locator($".village1 a.buildingSlot{location}");
            return node;
        }

        private static ILocator GetInfrastructure(IPage page, int location)
        {
            if (location == 40) // wall
            {
                var node = page.Locator("#villageContent > div.buildingSlot.a40.top");
                return node;
            }

            var div = page.Locator($".village2 div.buildingSlot.a{location}");
            return div;
        }

        public static async Task<List<BuildingDto>> GetFields(IPage page)
        {
            var fields = page.Locator("#resourceFieldContainer a.level");
            var fieldCount = await fields.CountAsync();
            var extractedData = new List<BuildingDto>();
            for (var i = 0; i < fieldCount; i++)
            {
                var field = fields.Nth(i);
                string classAttr = await field.GetAttributeAsync("class") ?? "";
                var slotMatch = BuildingSlotExtractor().Match(classAttr);
                var gidMatch = BuildingTypeExtractor().Match(classAttr);
                var levelMatch = LevelExtractor().Match(classAttr);
                bool isUnderConstruction = classAttr.Contains("underConstruction");
                extractedData.Add(new BuildingDto
                {
                    Location = slotMatch.Success ? int.Parse(slotMatch.Groups[1].Value) : -1,
                    Type = gidMatch.Success ? (BuildingEnums)int.Parse(gidMatch.Groups[1].Value) : BuildingEnums.Unknown,
                    Level = levelMatch.Success ? int.Parse(levelMatch.Groups[1].Value) : -2,
                    IsUnderConstruction = isUnderConstruction
                });
            }
            return extractedData;
        }

        public static async Task<List<BuildingDto>> GetInfrastructures(IPage page)
        {
            var buildings = page.Locator("#villageContent .buildingSlot");
            var buildingCount = await buildings.CountAsync();
            var extractedData = new List<BuildingDto>();

            for (var i = 0; i < buildingCount; i++)
            {
                if (i == 22) continue;
                var building = buildings.Nth(i);
                int location = await building.GetAttributeAsync("data-aid") is string locStr && int.TryParse(locStr, out int loc) ? loc : -1;
                int level = await building.Locator("a").GetAttributeAsync("data-level") is string levelStr && int.TryParse(levelStr, out int l) ? l : -1;
                bool isUnderConstruction = await building.Locator("a").EvaluateAsync<bool>("node => node.classList.contains('underConstruction')");

                var type = location switch
                {
                    26 => BuildingEnums.MainBuilding,
                    39 => BuildingEnums.RallyPoint,
                    _ => (BuildingEnums)(await building.GetAttributeAsync("data-gid") is string typeStr && int.TryParse(typeStr, out int t) ? t : -1)
                };

                extractedData.Add(new BuildingDto
                {
                    Location = location,
                    Type = type,
                    Level = level,
                    IsUnderConstruction = isUnderConstruction
                });
            }
            return extractedData;
        }

        public static async Task<List<QueueBuildingDto>> GetQueueBuilding(IPage page)
        {
            var buildings = page.Locator(".buildingList li");
            var buildingCount = await buildings.CountAsync();
            var extractedData = new List<QueueBuildingDto>();

            for (var i = 0; i < buildingCount; i++)
            {
                var building = buildings.Nth(i);
                string type = await building.Locator(".name").InnerTextAsync();
                int level = await building.Locator(".lvl").InnerTextAsync() is string levelStr && int.TryParse(levelStr, out int l) ? l : 0;
                int durationSeconds = await building.Locator(".timer").GetAttributeAsync("value") is string durationStr && int.TryParse(durationStr, out int d) ? d : 0;
                extractedData.Add(new QueueBuildingDto
                {
                    Type = type,
                    Level = level,
                    CompleteTime = DateTime.Now.AddSeconds(durationSeconds),
                    Location = -1
                });
            }

            return extractedData;
        }

        public static async Task<int> CountQueueBuilding(IPage page)
        {
            var buildings = page.Locator(".buildingList li");
            var buildingCount = await buildings.CountAsync();
            return buildingCount;
        }

        [GeneratedRegex(@"buildingSlot(\d+)")]
        private static partial Regex BuildingSlotExtractor();

        [GeneratedRegex(@"gid(\d+)")]
        private static partial Regex BuildingTypeExtractor();

        [GeneratedRegex(@"\blevel(\d+)")]
        private static partial Regex LevelExtractor();
    }
}