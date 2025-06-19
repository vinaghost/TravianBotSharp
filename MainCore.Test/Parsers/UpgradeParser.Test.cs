using MainCore.Enums;

namespace MainCore.Test.Parsers
{
    public class UpgradeParser : BaseParser
    {
        private const string CrannyEmpty = "Parsers/Upgrade/CrannyEmpty.html";
        private const string MarketplaceConstructed = "Parsers/Upgrade/MarketplaceConstructed.html";
        private const string MarketplaceWithoutResource = "Parsers/Upgrade/MarketplaceWithoutResource.html";
        private const string CroplandConstructed = "Parsers/Upgrade/CroplandConstructed.html";
        private const string CroplandEmpty = "Parsers/Upgrade/CroplandEmpty.html";
        private const string CroplandUpgradingSingle = "Parsers/Upgrade/CroplandUpgradingSingle.html";
        private const string CroplandUpgradingDouble = "Parsers/Upgrade/CroplandUpgradingDouble.html";

        [Theory]
        [InlineData(CrannyEmpty, BuildingEnums.Cranny)]
        [InlineData(MarketplaceConstructed, BuildingEnums.Marketplace)]
        [InlineData(MarketplaceWithoutResource, BuildingEnums.Marketplace)]
        [InlineData(CroplandConstructed, BuildingEnums.Cropland)]
        [InlineData(CroplandEmpty, BuildingEnums.Cropland)]
        public void GetRequiredResource(string path, BuildingEnums building)
        {
            _html.Load(path);
            var actual = MainCore.Parsers.UpgradeParser.GetRequiredResource(_html, building);
            actual.Count.ShouldBe(5);
        }

        [Fact]
        public void GetTimeWhenEnoughResource()
        {
            _html.Load(MarketplaceWithoutResource);
            var actual = MainCore.Parsers.UpgradeParser.GetTimeWhenEnoughResource(_html, BuildingEnums.Marketplace);
            actual.ShouldBeGreaterThan(TimeSpan.Zero);
        }

        [Theory]
        [InlineData(CrannyEmpty, BuildingEnums.Cranny)]
        [InlineData(CroplandEmpty, BuildingEnums.Cropland)]
        public void GetConstructButton(string path, BuildingEnums building)
        {
            _html.Load(path);
            var actual = MainCore.Parsers.UpgradeParser.GetConstructButton(_html, building);
            actual.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(MarketplaceConstructed)]
        [InlineData(CroplandConstructed)]
        public void GetUpgradeButton(string path)
        {
            _html.Load(path);
            var actual = MainCore.Parsers.UpgradeParser.GetUpgradeButton(_html);
            actual.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(MarketplaceConstructed)]
        [InlineData(CroplandConstructed)]
        public void GetSpecialUpgradeButton(string path)
        {
            _html.Load(path);
            var actual = MainCore.Parsers.UpgradeParser.GetSpecialUpgradeButton(_html);
            actual.ShouldNotBeNull();
        }

        [Fact]
        public void GetUpgradingLevel_Single()
        {
            _html.Load(CroplandUpgradingSingle);
            var actual = MainCore.Parsers.UpgradeParser.GetUpgradingLevel(_html);
            actual.ShouldBe(9);
        }

        [Fact]
        public void GetUpgradingLevel_Multiple()
        {
            _html.Load(CroplandUpgradingDouble);
            var actual = MainCore.Parsers.UpgradeParser.GetUpgradingLevel(_html);
            actual.ShouldBe(10);
        }

        [Fact]
        public void GetNextLevel_NoButton()
        {
            _html.Load(CrannyEmpty);
            var actual = MainCore.Parsers.UpgradeParser.GetNextLevel(_html, BuildingEnums.Cranny);
            actual.ShouldBeNull();
        }

        [Theory]
        [InlineData(MarketplaceConstructed, BuildingEnums.Marketplace, 2)]
        [InlineData(CroplandConstructed, BuildingEnums.Cropland, 2)]
        public void GetNextLevel_ShouldParse(string path, BuildingEnums building, int expected)
        {
            _html.Load(path);
            var actual = MainCore.Parsers.UpgradeParser.GetNextLevel(_html, building);
            actual.ShouldBe(expected);
        }
    }
}