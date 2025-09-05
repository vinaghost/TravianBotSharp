using MainCore.Entities;

namespace MainCore.Test.Parsers
{
    public class FarmListParser : BaseParser
    {
        private const string FarmListPage = "Parsers/FarmList/FarmListPage.html";

        [Fact]
        public void GetFarmNodes()
        {
            _html.Load(FarmListPage);
            var actual = MainCore.Parsers.FarmListParser.GetFarmNodes(_html);
            actual.ShouldNotBeEmpty();
        }

        [Fact]
        public void GetId()
        {
            _html.Load(FarmListPage);
            var nodes = MainCore.Parsers.FarmListParser.GetFarmNodes(_html);
            var actual = MainCore.Parsers.FarmListParser.GetId(nodes.First());
            actual.ShouldNotBe(FarmId.Empty);
        }

        [Fact]
        public void GetName()
        {
            _html.Load(FarmListPage);
            var nodes = MainCore.Parsers.FarmListParser.GetFarmNodes(_html);
            var actual = MainCore.Parsers.FarmListParser.GetName(nodes.First());
            actual.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public void GetStartButton()
        {
            _html.Load(FarmListPage);

            var nodes = MainCore.Parsers.FarmListParser.GetFarmNodes(_html);
            var id = MainCore.Parsers.FarmListParser.GetId(nodes.First());
            var actual = MainCore.Parsers.FarmListParser.GetStartButton(_html, id);
            actual.ShouldNotBeNull();
        }

        [Fact]
        public void GetStartAllButton()
        {
            _html.Load(FarmListPage);
            var actual = MainCore.Parsers.FarmListParser.GetStartAllButton(_html);
            actual.ShouldNotBeNull();
        }
    }
}
