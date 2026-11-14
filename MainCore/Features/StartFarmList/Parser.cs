namespace MainCore.Features.StartFarmList
{
    public static class Parser
    {
        public record struct RawFarmDto(int FarmId, string Name);

        public static async Task<List<FarmDto>> GetFarmInfo(this IPage page)
        {
            var rawFarmsData = await page.EvaluateAsync<List<RawFarmDto>>(@"() => {
                const elements = document.querySelectorAll('#rallyPointFarmList div.farmListHeader');

                return Array.from(elements).map(header => {
                    const dragEl = header.querySelector('div.dragAndDrop');
                    const nameEl = header.querySelector('div.farmListName div.name');

                    const farmIdAttr = dragEl ? dragEl.getAttribute('data-list') : null;
                    const parsedId = farmIdAttr ? parseInt(farmIdAttr, 10) : -1;
                    const farmId = isNaN(parsedId) ? -1 : parsedId;

                    const name = nameEl ? nameEl.innerText.trim() : '';

                    return {
                        FarmId: farmId,
                        Name: name
                    };
                });
            }") ?? throw new InvalidOperationException("Failed to retrieve or parse farm data from the page.");

            var extractedFarms = new List<FarmDto>(rawFarmsData.Count);
            foreach (var raw in rawFarmsData)
            {
                extractedFarms.Add(new FarmDto
                {
                    Id = new FarmId(raw.FarmId),
                    Name = raw.Name
                });
            }

            return extractedFarms;
        }

        public static ILocator GetStartButton(this IPage page, FarmId raidId)
        {
            var button = page.Locator($"#rallyPointFarmList div.farmListHeader:has(div[data-list='{raidId.Value}']) button.startFarmList");
            return button;
        }

        public static ILocator GetStartAllButton(this IPage page)
        {
            var button = page.Locator("#rallyPointFarmList button.startAllFarmLists");
            return button;
        }
    }
}