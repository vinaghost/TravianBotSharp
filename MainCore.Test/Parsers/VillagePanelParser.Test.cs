using MainCore.Entities;

namespace MainCore.Test.Parsers
{
    public class VillagePanelParser : BaseParser
    {
        private const string Page = "Parsers/VillagePanel/Page.html";

        [Fact]
        public void GetCurrentVillageId()
        {
            _html.Load(Page);
            var actual = MainCore.Parsers.VillagePanelParser.GetCurrentVillageId(_html);
            actual.Should().BeGreaterThan(VillageId.Empty);
        }

        [Fact]
        public void GetVillageNode()
        {
            _html.Load(Page);
            var villageId = MainCore.Parsers.VillagePanelParser.GetCurrentVillageId(_html);
            var actual = MainCore.Parsers.VillagePanelParser.GetVillageNode(_html, villageId);
            actual.Should().NotBeNull();
        }
    }
}