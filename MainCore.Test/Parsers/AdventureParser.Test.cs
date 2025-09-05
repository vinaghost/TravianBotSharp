namespace MainCore.Test.Parsers
{
    public class AdventureParser : BaseParser
    {
        private const string AdventuresPage = "Parsers/Adventures/AdventuresPage.html";
        private const string AdventuresPageEmpty = "Parsers/Adventures/AdventuresPageEmpty.html";
        private const string AdventuresPageOnItsWay = "Parsers/Adventures/AdventuresPageOnItsWay.html";
        private const string ResourceFieldsPage = "Parsers/Adventures/ResourceFieldsPage.html";

        [Theory]
        [InlineData(AdventuresPage, true)]
        [InlineData(ResourceFieldsPage, false)]
        public void IsAdventurePage(string path, bool expected)
        {
            _html.Load(path);
            var actual = MainCore.Parsers.AdventureParser.IsAdventurePage(_html);
            actual.ShouldBe(expected);
        }

        [Fact]
        public void GetHeroAdventureButton()
        {
            _html.Load(AdventuresPage);
            var actual = MainCore.Parsers.AdventureParser.GetHeroAdventureButton(_html);
            actual.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(AdventuresPage, true)]
        [InlineData(AdventuresPageEmpty, false)]
        public void CanStartAdventure(string path, bool expected)
        {
            _html.Load(path);
            var actual = MainCore.Parsers.AdventureParser.CanStartAdventure(_html);
            actual.ShouldBe(expected);
        }

        [Fact]
        public void GetAdventureDuration()
        {
            _html.Load(AdventuresPageOnItsWay);
            var actual = MainCore.Parsers.AdventureParser.GetAdventureDuration(_html);
            actual.ShouldBeGreaterThan(TimeSpan.Zero);
        }

        [Fact]
        public void GetAdventureButton()
        {
            _html.Load(AdventuresPage);
            var actual = MainCore.Parsers.AdventureParser.GetAdventureButton(_html);
            actual.ShouldNotBeNull();
        }

        [Fact]
        public void GetContinueButton()
        {
            _html.Load(AdventuresPageOnItsWay);
            var actual = MainCore.Parsers.AdventureParser.GetContinueButton(_html);
            actual.ShouldNotBeNull();
        }

        [Fact]
        public void GetAdventureInfo()
        {
            _html.Load(AdventuresPage);
            var adventureButton = MainCore.Parsers.AdventureParser.GetAdventureButton(_html);
            adventureButton.ShouldNotBeNull();
            var actual = MainCore.Parsers.AdventureParser.GetAdventureInfo(adventureButton);
            actual.ShouldNotContain("unknown");
            actual.ShouldNotContain("[~|~]");
        }
    }
}
