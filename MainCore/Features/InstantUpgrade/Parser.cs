namespace MainCore.Features.InstantUpgrade
{
    public static class Parser
    {
        public static async Task<int> CountQueueBuilding(this IPage page)
        {
            var buildings = page.Locator(".buildingList li");
            var buildingCount = await buildings.CountAsync();
            return buildingCount;
        }

        public static ILocator GetCompleteButton(this IPage page)
        {
            var button = page.Locator(".buildingList .finishNow button");
            return button;
        }

        public static ILocator GetConfirmButton(this IPage page)
        {
            var button = page.Locator("#finishNowDialog button");
            return button;
        }
    }
}