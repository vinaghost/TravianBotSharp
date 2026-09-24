using Microsoft.Playwright;
using System.Text.RegularExpressions;

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

        [GeneratedRegex(@"item(\d+)")]
        private static partial Regex ItemExtractor();

        public static async Task<List<HeroItemDto>> GetItems(IPage page)
        {
            var cells = page.Locator($"div.heroItems div.heroItem:not(.empty)");
            var count = await cells.CountAsync();
            var extractedItems = new List<HeroItemDto>();

            for (var i = 0; i < count; i++)
            {
                var cell = cells.Nth(i);

                var itemSlot = cell.Locator(".item");
                var classes = await itemSlot.GetAttributeAsync("class") ?? "";

                var itemMatch = ItemExtractor().Match(classes);
                var item = itemMatch.Success ? (HeroItemEnums)int.Parse(itemMatch.Groups[1].Value) : HeroItemEnums.None;

                if (item == HeroItemEnums.None) continue;

                var dataTier = await cell.GetAttributeAsync("data-tier") ?? "";

                if (!dataTier.Contains("consumable"))
                {
                    extractedItems.Add(new HeroItemDto()
                    {
                        Type = item,
                        Amount = 1,
                    });
                    continue;
                }

                var countSlot = cell.Locator(".count");
                var exists = await countSlot.CountAsync() > 0;

                if (!exists)
                {
                    extractedItems.Add(new HeroItemDto()
                    {
                        Type = item,
                        Amount = 1,
                    });
                    continue;
                }

                var amountText = await countSlot.InnerTextAsync();
                var amount = amountText.ParseInt();

                extractedItems.Add(new HeroItemDto()
                {
                    Type = item,
                    Amount = amount > 0 ? amount : 1,
                });
            }
            return extractedItems;
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