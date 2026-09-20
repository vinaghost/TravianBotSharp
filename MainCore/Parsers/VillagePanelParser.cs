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

        public static async Task<List<VillageDto>> Get(IPage page)
        {
            var nodes = page.Locator("#sidebarBoxVillageList div.listEntry.village");
            var nodeCount = await nodes.CountAsync();
            var extractedVillages = new List<VillageDto>();

            for (int i = 0; i < nodeCount; i++)
            {
                var node = nodes.Nth(i);
                var id = await node.GetAttributeAsync("data-did");
                var name = await node.Locator("a span.name").InnerTextAsync();
                var x = await node.Locator("span.coordinateX").InnerTextAsync();
                var y = await node.Locator("span.coordinateY").InnerTextAsync();
                var isActive = await node.EvaluateAsync<bool>("node => node.classList.contains('active')");
                var isUnderAttack = await node.EvaluateAsync<bool>("node => node.classList.contains('attack')");
                extractedVillages.Add(new VillageDto
                {
                    Id = new VillageId(int.Parse(id ?? "0")),
                    Name = name,
                    X = x.ParseInt(),
                    Y = y.ParseInt(),
                    IsActive = isActive,
                    IsUnderAttack = isUnderAttack,
                });
            }
            return extractedVillages;
        }
    }
}