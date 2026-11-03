namespace MainCore.Parsers
{
    public static partial class BuildingLayoutParser
    {
        public static async Task<int> CountQueueBuilding(IPage page)
        {
            var buildings = page.Locator(".buildingList li");
            var buildingCount = await buildings.CountAsync();
            return buildingCount;
        }
    }
}