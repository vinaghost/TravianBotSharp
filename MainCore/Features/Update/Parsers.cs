using System.Text.Json;

namespace MainCore.Features.Update;

public static class Parsers
{
    public record struct RawHeroItemDto(string? ItemIdStr, string? DataTier, string? AmountText, bool HasCountSlot);

    public static async Task<List<HeroItemDto>> GetItems(this IPage page)
    {
        var jsonResult = await page.EvaluateAsync<JsonElement>(@"() => {
                const cells = document.querySelectorAll('div.heroItems div.heroItem:not(.empty)');
                const result = [];

                const itemRegex = /item(\d+)/;

                cells.forEach(cell => {
                    const itemSlot = cell.querySelector('.item');
                    const countSlot = cell.querySelector('.count');

                    const classes = itemSlot ? (itemSlot.getAttribute('class') || '') : '';
                    const itemMatch = classes.match(itemRegex);

                    result.push({
                        ItemIdStr: itemMatch ? itemMatch[1] : null,
                        DataTier: cell.getAttribute('data-tier') || '',
                        AmountText: countSlot ? countSlot.innerText.trim() : '',
                        HasCountSlot: !!countSlot
                    });
                });
                return result;
            }");
        var text = jsonResult.GetRawText();
        var rawItemsData = JsonSerializer.Deserialize<List<RawHeroItemDto>>(text) ?? throw new InvalidOperationException($"Failed to deserialize hero inventory data from the page. Content: {text}");

        var extractedItems = new List<HeroItemDto>();

        foreach (var raw in rawItemsData)
        {
            var item = int.TryParse(raw.ItemIdStr, out int itemId)
                ? (HeroItemEnums)itemId
                : HeroItemEnums.None;

            if (item == HeroItemEnums.None) continue;

            string dataTier = raw.DataTier ?? "";

            if (!dataTier.Contains("consumable"))
            {
                extractedItems.Add(new HeroItemDto
                {
                    Type = item,
                    Amount = 1,
                });
                continue;
            }

            if (!raw.HasCountSlot)
            {
                extractedItems.Add(new HeroItemDto
                {
                    Type = item,
                    Amount = 1,
                });
                continue;
            }

            int amount = int.TryParse(raw.AmountText, out int parsedAmt) ? parsedAmt : 0;

            extractedItems.Add(new HeroItemDto
            {
                Type = item,
                Amount = amount > 0 ? amount : 1,
            });
        }

        return extractedItems;
    }
}