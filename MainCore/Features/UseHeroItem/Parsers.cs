using System.Text.Json;

using MainCore.Features.UpgradeBuilding;

namespace MainCore.Features.UseHeroItem;

public static class Parsers
{
    public static ILocator GetInventoryPage(this IPage page)
    {
        var inventory = page.Locator("#heroV2 .inventoryPageWrapper .inventoryWrapper");
        return inventory;
    }

    public static ILocator GetInventoryPageWrapper(this IPage page)
    {
        var inventory = page.Locator("#heroV2 .inventoryPageWrapper");
        return inventory;
    }

    public static async Task<bool> IsInventoryPageLoaded(this IPage page)
    {
        var inventoryPageWrapper = page.Locator("#heroV2 .inventoryPageWrapper");
        var count = await inventoryPageWrapper.CountAsync();
        var loading = await inventoryPageWrapper.EvaluateAsync<bool>("node => node.classList.contains('loading')");
        return count > 0 && !loading;
    }

    public static async Task<long[]> GetInventoryResources(this IPage page)
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

    public static ILocator GetItemSlot(this IPage page, HeroItemEnums type)
    {
        var item = page.Locator($"div.heroItems div.heroItem:not(.empty):has(.item{(int)type})");
        return item;
    }

    public static ILocator GetAmountBox(this IPage page, string name)
    {
        var box = page.Locator($"div.resourceTransferDialog .resourceRow input[name='{name}']");
        return box;
    }

    public static ILocator GetConfirmButton(this IPage page)
    {
        var button = page.Locator("div.resourceTransferDialog .actionButton button:nth-of-type(2)");
        return button;
    }

    public static ILocator GetResourceConfirmButton(this IPage page)
    {
        var button = page.Locator("div.resourceTransferDialog .actionButton button");
        return button;
    }

    public static ILocator GetResourceTransferDialog(this IPage page)
    {
        var dialog = page.Locator("div.resourceTransferDialog");
        return dialog;
    }

    public static ILocator GetSuccessToast(this IPage page)
    {
        var toast = page.Locator("div.toast.toastSuccess");
        return toast;
    }

    public static async Task<ILocator> GetFillUpButton(this IPage page, BuildingEnums building)
    {
        var panel = await page.GetPanel(building);
        var node = panel.Locator("#contract .resourceWrapper .resource.transfer.fillUp");
        return node.First;
    }
}