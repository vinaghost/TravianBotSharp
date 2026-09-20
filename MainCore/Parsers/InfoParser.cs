using Microsoft.Playwright;

namespace MainCore.Parsers
{
    public static class InfoParser
    {
        public static async Task<int> GetGold(IPage page)
        {
            var goldNode = page.Locator("div.ajaxReplaceableGoldAmount");
            var gold = await goldNode.InnerTextAsync();
            return gold.ParseInt();
        }

        public static async Task<int> GetSilver(IPage page)
        {
            var silverNode = page.Locator("div.ajaxReplaceableSilverAmount");
            var silver = await silverNode.InnerTextAsync();
            return silver.ParseInt();
        }

        public static async Task<bool> HasPlusAccount(IPage page)
        {
            var editButton = page.Locator("#sidebarBoxLinklist a.edit.round");
            var classAttr = await editButton.GetAttributeAsync("class") ?? "";

            if (classAttr.Contains("green")) return true;
            if (classAttr.Contains("gold")) return false;
            return false;
        }
    }
}