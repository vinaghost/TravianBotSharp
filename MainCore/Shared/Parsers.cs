using System.Net;

namespace MainCore.Shared;

public static class Parsers
{
    public static ILocator GetHeroAvatar(this IPage page)
    {
        return page.Locator("#topBarHero #heroImageButton");
    }

    public static ILocator GetFieldLocator(this IPage page, BuildingEnums buildingType)
    {
        var node = page.Locator($".village1 a.gid{(int)buildingType}");
        return node;
    }

    public static ILocator GetFieldLocator(this IPage page, BuildingEnums buildingType, int location)
    {
        var node = page.Locator($".village1 a.buildingSlot{location}.gid{(int)buildingType}");
        return node;
    }

    public static ILocator GetBuildingLocator(this IPage page, BuildingEnums buildingType, int location)
    {
        var node = page.Locator($".village2 div.buildingSlot.g{(int)buildingType}.aid{location}");
        return node;
    }

    public static ILocator GetDorf(this IPage page, int dorf)
    {
        var node = page.Locator($".village{dorf}").First;
        return node;
    }

    public static ILocator GetDorf(this IPage page, BuildingEnums buildingType)
    {
        if (buildingType.IsResourceField())
        {
            return page.GetDorf(1);
        }
        return page.GetDorf(2);
    }

    public static ILocator GetButton(this IPage page, int key)
    {
        var button = page.Locator($"#navigation a[accesskey='{key}']");
        return button;
    }

    public static ILocator GetServerTime(this IPage page)
    {
        var serverTime = page.Locator("#servertime");
        return serverTime;
    }

    private static string Normalized(this string value)
    {
        var valueStrDecoded = WebUtility.HtmlDecode(value);
        if (string.IsNullOrEmpty(valueStrDecoded)) return "";

        var valueStr = new string(valueStrDecoded.Where(c => char.IsDigit(c) || c == '-' || c == '−').ToArray());
        valueStr = valueStr.Replace('−', '-');

        if (string.IsNullOrEmpty(valueStr)) return "";
        return valueStr;
    }

    public static int ParseInt(this string value)
    {
        var normValue = value.Normalized();
        if (string.IsNullOrEmpty(normValue)) return -1;
        return int.Parse(normValue);
    }

    public static long ParseLong(this string value)
    {
        var normValue = value.Normalized();
        if (string.IsNullOrEmpty(normValue)) return -1;
        return long.Parse(normValue);
    }
}