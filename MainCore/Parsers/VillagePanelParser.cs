using System.Text.Json;

namespace MainCore.Parsers
{
    public static class VillagePanelParser
    {
        public static ILocator GetVillageNode(IPage page, VillageId villageId)
        {
            var node = page.Locator($"#sidebarBoxVillageList div.listEntry.village[data-did='{villageId}']");
            return node;
        }

        public static async Task<VillageId> GetCurrentVillageId(IPage page)
        {
            var node = page.Locator("#sidebarBoxVillageList div.listEntry.village.active");
            var dataDid = await node.GetAttributeAsync("data-did");
            return new VillageId(int.Parse(dataDid ?? "0"));
        }

        public static async Task<bool> IsActive(ILocator locator)
        {
            return await locator.EvaluateAsync<bool>("node => node.classList.contains('active')");
        }

        public record struct RawVillageDto(string? IdStr, string? Name, string? CoordinateX, string? CoordinateY, bool IsActive, bool IsUnderAttack);

        public static async Task<List<VillageDto>> Get(IPage page)
        {
            var jsonResult = await page.EvaluateAsync<JsonElement>(@"() => {
                const elements = document.querySelectorAll('#sidebarBoxVillageList div.listEntry.village');
                const result = [];

                elements.forEach(node => {
                    const nameEl = node.querySelector('a span.name');
                    const xEl = node.querySelector('span.coordinateX');
                    const yEl = node.querySelector('span.coordinateY');

                    result.push({
                        IdStr: node.getAttribute('data-did') || '0',
                        Name: nameEl ? (nameEl.innerText || nameEl.textContent).trim() : '',
                        CoordinateX: xEl ? (xEl.innerText || xEl.textContent).trim() : '0',
                        CoordinateY: yEl ? (yEl.innerText || yEl.textContent).trim() : '0',
                        IsActive: node.classList.contains('active'),
                        IsUnderAttack: node.classList.contains('attack')
                    });
                });
                return result;
            }");

            var text = jsonResult.GetRawText();
            var rawVillagesData = JsonSerializer.Deserialize<List<RawVillageDto>>(text) ?? throw new InvalidOperationException($"Failed to deserialize building data from the page. Content: {text}");

            var extractedVillages = new List<VillageDto>();

            foreach (var raw in rawVillagesData)
            {
                int id = int.TryParse(raw.IdStr, out int parsedId) ? parsedId : 0;

                extractedVillages.Add(new VillageDto
                {
                    Id = new VillageId(id),
                    Name = raw.Name ?? "",
                    X = (raw.CoordinateX ?? "").ParseInt(),
                    Y = (raw.CoordinateY ?? "").ParseInt(),
                    IsActive = raw.IsActive,
                    IsUnderAttack = raw.IsUnderAttack
                });
            }

            return extractedVillages;
        }
    }
}