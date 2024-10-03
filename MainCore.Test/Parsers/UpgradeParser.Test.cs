using MainCore.Common.Enums;

namespace MainCore.Test.Parsers
{
    public class UpgradeParser : BaseParser
    {
        private const string CrannyEmpty = "Parsers/Upgrade/CrannyEmpty.html";
        private const string MarketplaceConstructed = "Parsers/Upgrade/MarketplaceConstructed.html";
        private const string MarketplaceWithoutResource = "Parsers/Upgrade/MarketplaceWithoutResource.html";
        private const string CroplandConstructed = "Parsers/Upgrade/CroplandConstructed.html";
        private const string CroplandEmpty = "Parsers/Upgrade/CroplandEmpty.html";

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
            actual.Count.Should().Be(5);
        }

        [Fact]
        public void GetTimeWhenEnoughResource()
        {
            _html.Load(MarketplaceWithoutResource);
            var actual = MainCore.Parsers.UpgradeParser.GetTimeWhenEnoughResource(_html, BuildingEnums.Marketplace);
            actual.Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Theory]
        [InlineData(CrannyEmpty, BuildingEnums.Cranny)]
        [InlineData(CroplandEmpty, BuildingEnums.Cropland)]
        public void GetConstructButton(string path, BuildingEnums building)
        {
            _html.Load(path);
            var actual = MainCore.Parsers.UpgradeParser.GetConstructButton(_html, building);
            actual.Should().NotBeNull();
        }

        [Theory]
        [InlineData(MarketplaceConstructed)]
        [InlineData(CroplandConstructed)]
        public void GetUpgradeButton(string path)
        {
            _html.Load(path);
            var actual = MainCore.Parsers.UpgradeParser.GetUpgradeButton(_html);
            actual.Should().NotBeNull();
        }

        [Theory]
        [InlineData(MarketplaceConstructed)]
        [InlineData(CroplandConstructed)]
        public void GetSpecialUpgradeButton(string path)
        {
            _html.Load(path);
            var actual = MainCore.Parsers.UpgradeParser.GetSpecialUpgradeButton(_html);
            actual.Should().NotBeNull();
        }
    }
}