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
        var data = await page.EvaluateAsync<long[]>(@"() => {
                const cells = document.querySelectorAll('div.heroItem');
                const result = [];
                cells.forEach(cell => {
                    const countSlot = cell.querySelector('.count');
                    result.push(countSlot ? parseInt(countSlot.innerText, 10) : 0);
                });
                return result;
            }");
        return data;
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