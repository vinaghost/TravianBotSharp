namespace MainCore.Features.SetHeroPoint;

public static class Parsers
{
    public static ILocator GetHeroLevelUpMark(this IPage page)
    {
        return page.Locator("#topBarHero i.levelUp.show");
    }

    public static ILocator GetHeroPanel(this IPage page)
    {
        return page.Locator("#heroV2");
    }

    public static ILocator GetHeroAttributeBoxes(this IPage page)
    {
        return page.Locator("#heroV2 .attributeBox .heroAttributes .attributes.formV2");
    }

    public static ILocator GetHeroAttributeBox(this IPage page, string name)
    {
        return page.Locator($"#heroV2 .attributeBox .heroAttributes .attributes.formV2 input[name='{name}']");
    }

    public static ILocator GetHeroAttributeSaveButton(this IPage page)
    {
        return page.Locator("#heroV2 .attributeBox .heroAttributes .attributes.formV2 button#savePoints");
    }
}