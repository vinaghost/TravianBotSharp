using Microsoft.Playwright;

namespace MainCore.Parsers
{
    public static class CompleteImmediatelyParser
    {
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

        public static ILocator GetCompleteButton(IPage page)
        {
            var button = page.Locator(".buildingList .finishNow button");
            return button;
        }

        public static ILocator GetConfirmButton(IPage page)
        {
            var button = page.Locator("#finishNowDialog button");
            return button;
        }
    }
}