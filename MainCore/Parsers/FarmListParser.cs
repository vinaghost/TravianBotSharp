using Microsoft.Playwright;

namespace MainCore.Parsers
{
    public static class FarmListParser
    {
        public static async Task<List<FarmDto>> GetFarmInfo(IPage page)
        {
            var farmListHeaders = page.Locator("#rallyPointFarmList div.farmListHeader");
            var farmListHeaderCount = await farmListHeaders.CountAsync();

            var extractedFarms = new List<FarmDto>();
            for (var i = 0; i < farmListHeaderCount; i++)
            {
                var farmListHeader = farmListHeaders.Nth(i);
                var farmId = await farmListHeader.Locator("div.dragAndDrop").GetAttributeAsync("data-list") is string farmIdStr && int.TryParse(farmIdStr, out int id) ? id : -1;
                var name = await farmListHeader.Locator("div.farmListName div.name").InnerTextAsync();

                extractedFarms.Add(new FarmDto
                {
                    Id = new FarmId(farmId),
                    Name = name.Trim()
                });
            }
            return extractedFarms;
        }

        public static ILocator GetStartButton(IPage page, FarmId raidId)
        {
            var button = page.Locator($"#rallyPointFarmList div.farmListHeader:has(div[data-list='{raidId.Value}']) button.startFarmList");
            return button;
        }

        public static ILocator GetStartAllButton(IPage page)
        {
            var button = page.Locator("#rallyPointFarmList button.startAllFarmLists");
            return button;
        }
    }
}