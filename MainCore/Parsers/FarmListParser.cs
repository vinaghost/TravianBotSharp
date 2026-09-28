using Microsoft.Playwright;
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
    }
}