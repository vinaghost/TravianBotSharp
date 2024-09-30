using FluentAssertions;

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
            actual.Should().Be(expected);
        }

        [Fact]
        public void GetHeroAdventureButton()
        {
            _html.Load(AdventuresPage);
            var actual = MainCore.Parsers.AdventureParser.GetHeroAdventureButton(_html);
            actual.Should().NotBeNull();
        }

        [Theory]
        [InlineData(AdventuresPage, true)]
        [InlineData(AdventuresPageEmpty, false)]
        public void CanStartAdventure(string path, bool expected)
        {
            _html.Load(path);
            var actual = MainCore.Parsers.AdventureParser.CanStartAdventure(_html);
            actual.Should().Be(expected);
        }

        [Fact]
        public void GetAdventureDuration()
        {
            _html.Load(AdventuresPageOnItsWay);
            var actual = MainCore.Parsers.AdventureParser.GetAdventureDuration(_html);
            actual.Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Fact]
        public void GetAdventure()
        {
            _html.Load(AdventuresPage);
            var actual = MainCore.Parsers.AdventureParser.GetAdventure(_html);
            actual.Should().NotBeNull();
        }

        [Fact]
        public void GetContinueButton()
        {
            _html.Load(AdventuresPageOnItsWay);
            var actual = MainCore.Parsers.AdventureParser.GetContinueButton(_html);
            actual.Should().NotBeNull();
        }

        [Fact]
        public void GetAdventureInfo()
        {
            _html.Load(AdventuresPage);
            var adventure = MainCore.Parsers.AdventureParser.GetAdventure(_html);
            var actual = MainCore.Parsers.AdventureParser.GetAdventureInfo(adventure);
            actual.Should().NotContain("unknown");
            actual.Should().NotContain("[~|~]");
        }
    }
}