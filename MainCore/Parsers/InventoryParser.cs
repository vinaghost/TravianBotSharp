using Microsoft.Playwright;

namespace MainCore.Parsers
{
    public static class InventoryParser
    {
        public static ILocator GetInventoryPage(IPage page)
        {
            var inventory = page.Locator("#heroV2 .inventoryPageWrapper .inventoryWrapper");
            return inventory;
        }

        public static async Task<bool> IsInventoryPageLoaded(IPage page)
        {
            var inventoryPageWrapper = page.Locator("#heroV2 .inventoryPageWrapper");
            var count = await inventoryPageWrapper.CountAsync();
            var loading = await inventoryPageWrapper.EvaluateAsync<bool>("node => node.classList.contains('loading')");
            return count > 0 && !loading;
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