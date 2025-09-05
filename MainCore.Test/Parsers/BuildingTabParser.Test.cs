namespace MainCore.Test.Parsers
{
    public class BuildingTabParser : BaseParser
    {
        private const string HeroPage = "Parsers/BuildingTab/HeroPage.html";
        private const string Marketplace = "Parsers/BuildingTab/Marketplace.html";
        private const string NewBuilding = "Parsers/BuildingTab/NewBuilding.html";
        private const string Rallypoint = "Parsers/BuildingTab/Rallypoint.html";

        [Theory]
        [InlineData(HeroPage)]
        [InlineData(Marketplace)]
        [InlineData(NewBuilding)]
        [InlineData(Rallypoint)]
        public void GetNavigationBar(string path)
        {
            _html.Load(path);
            var actual = MainCore.Parsers.BuildingTabParser.GetNavigationBar(_html);
            actual.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(HeroPage, 3)]
        [InlineData(Marketplace, 5)]
        [InlineData(NewBuilding, 3)]
        [InlineData(Rallypoint, 5)]
        public void CountTab(string path, int expected)
        {
            _html.Load(path);
            var actual = MainCore.Parsers.BuildingTabParser.CountTab(_html);
            actual.ShouldBe(expected);
        }

        [Theory]
        [InlineData(HeroPage, 0)]
        [InlineData(Marketplace, 0)]
        [InlineData(NewBuilding, 0)]
        [InlineData(Rallypoint, 2)]
        public void IsTabActive(string path, int expected)
        {
            _html.Load(path);
            var node = MainCore.Parsers.BuildingTabParser.GetTab(_html, expected);
            var actual = MainCore.Parsers.BuildingTabParser.IsTabActive(node);
            actual.ShouldBeTrue();
        }
    }
}
