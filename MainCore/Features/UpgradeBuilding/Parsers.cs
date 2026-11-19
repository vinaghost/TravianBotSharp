namespace MainCore.Features.UpgradeBuilding;

public static class Parsers
{
    public static async Task<bool> IsBuildingExist(this IPage page, BuildingEnums buildingType, int location)
    {
        if (buildingType.IsResourceField())
        {
            var field = page.GetFieldLocator(buildingType, location);
            return await field.CountAsync() > 0;
        }
        else
        {
            var building = page.GetBuildingLocator(buildingType, location);
            return await building.CountAsync() > 0;
        }
    }

    public static async Task<int> GetLevel(this IPage page, BuildingEnums buildingType, int location)
    {
        if (buildingType.IsResourceField())
        {
            var field = page.GetFieldLocator(buildingType, location).Locator("div.labelLayer");
            var levelStr = await field.InnerTextAsync();
            var levelInt = int.TryParse(levelStr, out var parsedLevel) ? parsedLevel : 0;
            return levelInt;
        }
        else
        {
            var building = page.GetBuildingLocator(buildingType, location).Locator("a.level");
            var levelStr = await building.GetAttributeAsync("data-level");
            var levelInt = int.TryParse(levelStr, out var parsedLevel) ? parsedLevel : 0;
            return levelInt;
        }
    }

    public static async Task<bool> IsUnderConstruction(this IPage page, BuildingEnums buildingType, int location)
    {
        if (buildingType.IsResourceField())
        {
            var field = page.GetFieldLocator(buildingType, location);
            var classes = await field.GetAttributeAsync("class");
            if (string.IsNullOrEmpty(classes)) return false;
            return classes.Contains("underConstruction");
        }
        else
        {
            var building = page.GetBuildingLocator(buildingType, location).Locator("a.level");
            var classes = await building.GetAttributeAsync("class");
            if (string.IsNullOrEmpty(classes)) return false;
            return classes.Contains("underConstruction");
        }
    }

    public static async Task<ILocator> GetPanel(this IPage page, BuildingEnums building)
    {
        if (await IsEmptySite(page))
        {
            return page.Locator($"#contract_building{(int)building} div.upgradeBuilding");
        }
        else
        {
            return page.Locator("div.upgradeBuilding");
        }
    }

    public static async Task<ILocator> GetRequiredResource(this IPage page, BuildingEnums building)
    {
        var panel = await page.GetPanel(building);
        var nodes = panel.Locator("#contract div.resourceWrapper div.resource");
        return nodes;
    }

    public static async Task<TimeSpan> GetTimeWhenEnoughResource(this IPage page, BuildingEnums building)
    {
        var panel = await page.GetPanel(building);
        var node = panel.Locator("#contract div.errorMessage span.timer");
        var timeValue = await node.GetAttributeAsync("value");
        return TimeSpan.FromSeconds(int.Parse(timeValue ?? "0"));
    }

    public static async Task<ILocator> GetNormalButton(this IPage page, BuildingEnums building)
    {
        if (await IsEmptySite(page))
        {
            return page.Locator($"#contract_building{(int)building} div.upgradeButtonsContainer .section1 button.new");
        }
        else
        {
            return page.Locator("div.upgradeButtonsContainer .section1 button.build");
        }
    }

    public static async Task<ILocator> GetSpecialButton(this IPage page, BuildingEnums building)
    {
        if (await IsEmptySite(page))
        {
            return page.Locator($"#contract_building{(int)building} div.upgradeButtonsContainer .section2 button.videoFeatureButton");
        }
        else
        {
            return page.Locator("div.upgradeButtonsContainer .section2 button.videoFeatureButton");
        }
    }

    private static async Task<bool> IsEmptySite(this IPage page)
    {
        var parentPanel = page.Locator("#build");
        var classes = await parentPanel.GetAttributeAsync("class");
        return classes is not null && classes.Contains("gid0");
    }
}