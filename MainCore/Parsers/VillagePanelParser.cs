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
    }
}