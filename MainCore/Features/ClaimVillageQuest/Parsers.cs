namespace MainCore.Features.ClaimVillageQuest;

public static class Parsers
{
    public static ILocator GetQuestMaster(this IPage page)
    {
        var button = page.Locator("#questmasterButton");
        return button;
    }

    public static async Task<bool> IsVillageQuestClaimable(this IPage page)
    {
        var speechBubble = page.Locator("#questmasterButton div.newQuestSpeechBubble");
        return await speechBubble.CountAsync() > 0;
    }

    public static ILocator GetVillageQuestCollectButton(this IPage page)
    {
        var buttons = page.Locator("div.taskOverview button.collect:not(.disabled)");
        return buttons;
    }

    public static ILocator GetVillageQuestItems(this IPage page)
    {
        var items = page.Locator("#tasks .taskOverview .task .taskImage");
        return items;
    }
}