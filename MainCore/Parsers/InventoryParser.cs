using System.Text.Json;

namespace MainCore.Parsers
{
    public static partial class InventoryParser
    {
        public static ILocator GetInventoryPage(IPage page)
        {
            var inventory = page.Locator("#heroV2 .inventoryPageWrapper .inventoryWrapper");
            return inventory;
        }

        public static ILocator GetInventoryPageWrapper(IPage page)
        {
            var inventory = page.Locator("#heroV2 .inventoryPageWrapper");
            return inventory;
        }

        public static async Task<bool> IsInventoryPageLoaded(IPage page)
        {
            var inventoryPageWrapper = page.Locator("#heroV2 .inventoryPageWrapper");
            var count = await inventoryPageWrapper.CountAsync();
            var loading = await inventoryPageWrapper.EvaluateAsync<bool>("node => node.classList.contains('loading')");
            return count > 0 && !loading;
        }

        public record struct RawHeroItemDto(string? ItemIdStr, string? DataTier, string? AmountText, bool HasCountSlot);

        public static async Task<List<HeroItemDto>> GetItems(IPage page)
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

                // Non-consumables always have an amount of 1
                if (!dataTier.Contains("consumable"))
                {
                    extractedItems.Add(new HeroItemDto
                    {
                        Type = item,
                        Amount = 1,
                    });
                    continue;
                }

                // Consumables without a visible count badge also default to 1
                if (!raw.HasCountSlot)
                {
                    extractedItems.Add(new HeroItemDto
                    {
                        Type = item,
                        Amount = 1,
                    });
                    continue;
                }

                // Parse amount text (using standard int parsing or your .ParseInt() extension method)
                int amount = int.TryParse(raw.AmountText, out int parsedAmt) ? parsedAmt : 0;

                extractedItems.Add(new HeroItemDto
                {
                    Type = item,
                    Amount = amount > 0 ? amount : 1,
                });
            }

            return extractedItems;
        }

        public static async Task<long[]> GetInventoryResources(IPage page)
        {
            var jsonResult = await page.EvaluateAsync<JsonElement>(@"() => {
                const cells = document.querySelectorAll('div.heroItem');
                const result = [];

                function parseCount(text) {
                if (!text) return 0;
                const s = text.trim().toLowerCase();
                if (s === '') return 0;

                const suffixMatch = s.match(/^([\d,.]+)([km]?)$/i);
                if (!suffixMatch) {
                    // fallback: remove non-digits
                    const digits = s.replace(/[^0-9.]/g, '');
                    return digits ? Math.round(parseFloat(digits)) : 0;
                }
                let num = parseFloat(suffixMatch[1].replace(/,/g,''));
                const suffix = suffixMatch[2];
                if (suffix === 'k') num *= 1_000;
                else if (suffix === 'm') num *= 1_000_000;
                return Math.round(num);
                }

                cells.forEach(cell => {
                const countSlot = cell.querySelector('.count');
                result.push(countSlot ? parseCount(countSlot.innerText) : 0);
                });
                return result;
            }");
            var text = jsonResult.GetRawText();
            var rawItemsData = JsonSerializer.Deserialize<long[]>(text) ?? throw new InvalidOperationException($"Failed to deserialize inventory resource from the page. Content: {text}");
            return rawItemsData;
        }

        public static ILocator GetHeroAvatar(IPage page)
        {
            return page.Locator("#heroImageButton");
        }

        public static ILocator GetItemSlot(IPage page, HeroItemEnums type)
        {
            var item = page.Locator($"div.heroItems div.heroItem:not(.empty):has(.item{(int)type})");
            return item;
        }

        public static ILocator GetAmountBox(IPage page, string name)
        {
            var box = page.Locator($"div.resourceTransferDialog .resourceRow input[name='{name}']");
            return box;
        }

        public static ILocator GetConfirmButton(IPage page)
        {
            var button = page.Locator("div.resourceTransferDialog .actionButton button:nth-of-type(2)");
            return button;
        }

        public static ILocator GetResourceConfirmButton(IPage page)
        {
            var button = page.Locator("div.resourceTransferDialog .actionButton button");
            return button;
        }

        public static ILocator GetResourceTransferDialog(IPage page)
        {
            var dialog = page.Locator("div.resourceTransferDialog");
            return dialog;
        }

        public static ILocator GetSuccessToast(IPage page)
        {
            var toast = page.Locator("div.toast.toastSuccess");
            return toast;
        }
    }
}