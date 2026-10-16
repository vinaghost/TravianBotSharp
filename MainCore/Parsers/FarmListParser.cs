using System.Text.Json;

namespace MainCore.Parsers
{
    public static class FarmListParser
    {
        public record struct RawFarmDto(string? FarmIdStr, string? Name);

        public static async Task<List<FarmDto>> GetFarmInfo(IPage page)
        {
            var jsonResult = await page.EvaluateAsync<JsonElement>(@"() => {
                const elements = document.querySelectorAll('#rallyPointFarmList div.farmListHeader');
                const result = [];

                elements.forEach(header => {
                    const dragEl = header.querySelector('div.dragAndDrop');
                    const nameEl = header.querySelector('div.farmListName div.name');

                    result.push({
                        FarmIdStr: dragEl ? (dragEl.getAttribute('data-list') || '') : '',
                        Name: nameEl ? nameEl.innerText.trim() : ''
                    });
                });
                return result;
            }");

            var text = jsonResult.GetRawText();
            var rawFarmsData = JsonSerializer.Deserialize<List<RawFarmDto>>(text) ?? throw new InvalidOperationException($"Failed to deserialize farm data from the page. Content: {text}");

            var extractedFarms = new List<FarmDto>();

            foreach (var raw in rawFarmsData)
            {
                int farmId = int.TryParse(raw.FarmIdStr, out int id) ? id : -1;

                extractedFarms.Add(new FarmDto
                {
                    Id = new FarmId(farmId),
                    Name = (raw.Name ?? "").Trim()
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

        public record struct RawTargetDto(int X, int Y);

        /// <summary>
        /// Coordinates of the targets already in a farm list, read from each slot's map link.
        /// </summary>
        public static async Task<List<(int X, int Y)>> GetTargetCoordinates(IPage page, FarmId farmId)
        {
            var jsonResult = await page.EvaluateAsync<JsonElement>(@"(farmId) => {
                const result = [];
                document.querySelectorAll(`#rallyPointFarmList tr.slot:has(input[data-farm-list-id='${farmId}'])`).forEach(row => {
                    const href = row.querySelector('td.target a')?.getAttribute('href') || '';
                    const match = href.match(/x=(-?\d+)&y=(-?\d+)/);
                    if (match) result.push({ X: parseInt(match[1]), Y: parseInt(match[2]) });
                });
                return result;
            }", farmId.Value);

            var targets = JsonSerializer.Deserialize<List<RawTargetDto>>(jsonResult.GetRawText()) ?? [];
            return targets.Select(x => (x.X, x.Y)).ToList();
        }

        public static ILocator GetAddTargetButton(IPage page)
        {
            return page.Locator("#rallyPointFarmList td.addTarget a").First;
        }

        public static ILocator GetTargetForm(IPage page)
        {
            return page.Locator("#farmListTargetForm");
        }

        public static ILocator GetTargetListSelect(IPage page)
        {
            return page.Locator("#farmListTargetForm select[name='listId']");
        }

        public static ILocator GetTargetXInput(IPage page)
        {
            return page.Locator("#farmListTargetForm input[name='x']");
        }

        public static ILocator GetTargetYInput(IPage page)
        {
            return page.Locator("#farmListTargetForm input[name='y']");
        }

        public static ILocator GetTargetTroopInput(IPage page, int troopSlot)
        {
            return page.Locator($"#farmListTargetForm input[name='t{troopSlot}']");
        }

        public static ILocator GetTargetSaveButton(IPage page)
        {
            return page.Locator("#farmListTargetForm button.save");
        }

        public static ILocator GetTargetCancelButton(IPage page)
        {
            return page.Locator("#farmListTargetForm button.cancel");
        }
    }
}