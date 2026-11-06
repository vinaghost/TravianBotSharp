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

        public static ILocator GetFieldButton(IPage page, int location)
        {
            var node = page.Locator($".village1 a.buildingSlot{location}");
            return node;
        }

        public static ILocator GetBuildingButton(IPage page, int location)
        {
            var node = page.Locator($".village2 div.buildingSlot.a{location} svg path");
            return node.First;
        }

        private static ILocator GetFieldLocator(IPage page, BuildingEnums buildingType, int location)
        {
            var node = page.Locator($".village1 a.buildingSlot{location}.gid{buildingType}");
            return node;
        }

        private static ILocator GetFieldLocator(IPage page, BuildingEnums buildingType)
        {
            var node = page.Locator($".village1 a.gid{buildingType}");
            return node;
        }

        private static ILocator GetBuildingLocator(IPage page, BuildingEnums buildingType, int location)
        {
            var node = page.Locator($".village2 div.buildingSlot.g{buildingType}.aid{location}");
            return node;
        }

        public static ILocator GetBuildingLocator(IPage page, BuildingEnums buildingType)
        {
            return page.Locator($".village2 div.buildingSlot.g{buildingType}");
        }

        public static async Task<bool> IsBuildingLocationExist(IPage page, BuildingEnums buildingType, int location)
        {
            if (buildingType.IsResourceField())
            {
                var field = GetFieldLocator(page, buildingType, location);
                return await field.CountAsync() > 0;
            }
            else
            {
                var building = GetBuildingLocator(page, buildingType, location);
                return await building.CountAsync() > 0;
            }
        }

        public static async Task<bool> IsBuildingExist(IPage page, BuildingEnums buildingType)
        {
            if (buildingType.IsResourceField())
            {
                var field = GetFieldLocator(page, buildingType);
                return await field.CountAsync() > 0;
            }
            else
            {
                var building = GetBuildingLocator(page, buildingType);
                return await building.CountAsync() > 0;
            }
        }

        public static async Task<int> GetLevel(IPage page, BuildingEnums buildingType, int location)
        {
            if (buildingType.IsResourceField())
            {
                var field = GetFieldLocator(page, buildingType, location).Locator("div.labelLayer");
                var levelStr = await field.InnerTextAsync();
                var levelInt = int.TryParse(levelStr, out var parsedLevel) ? parsedLevel : 0;
                return levelInt;
            }
            else
            {
                var building = GetBuildingLocator(page, buildingType, location).Locator("a.level");
                var levelStr = await building.GetAttributeAsync("data-level");
                var levelInt = int.TryParse(levelStr, out var parsedLevel) ? parsedLevel : 0;
                return levelInt;
            }
        }

        public static async Task<bool> IsUnderConstruction(IPage page, BuildingEnums buildingType, int location)
        {
            if (buildingType.IsResourceField())
            {
                var field = GetFieldLocator(page, buildingType, location);
                var classes = await field.GetAttributeAsync("class");
                if (string.IsNullOrEmpty(classes)) return false;
                return classes.Contains("underConstruction");
            }
            else
            {
                var building = GetBuildingLocator(page, buildingType, location).Locator("a.level");
                var classes = await building.GetAttributeAsync("class");
                if (string.IsNullOrEmpty(classes)) return false;
                return classes.Contains("underConstruction");
            }
        }

        public static async Task<int> GetBuildingLocation(ILocator locator)
        {
            var location = await locator.GetAttributeAsync("data-aid");
            return int.Parse(location ?? "0");
        }
    }
}