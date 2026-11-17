namespace MainCore.Features.ClaimDailyQuest;

public static class Parsers
{
    public static async Task<bool> IsDailyQuestClaimable(this IPage page)
    {
        var indicator = page.Locator("#navigation .dailyQuests .indicator:not(.hidden)");
        return await indicator.CountAsync() > 0;
    }

    public static ILocator GetDailyQuestButton(this IPage page)
    {
        return page.GetButton(7);
    }

    public static ILocator GetDailyQuestItems(this IPage page)
    {
        return page.Locator("#dailyQuests .screen.active .dailyQuest:not(.placeholder)");
    }

    public static ILocator GetDailyQuestCollectButton(this IPage page)
    {
        var buttons = page.Locator("#dailyQuests .screen.active .dailyProgress div.reward:has(div.rewardImage.achieved) svg.above #innerCircle path");
        return buttons;
    }

    public static ILocator GetDailyQuestClaimButton(this IPage page)
    {
        var button = page.Locator("#dailyQuests .screen.active #dailyQuestsRewardScreen button.collect.collectable");
        return button;
    }
}