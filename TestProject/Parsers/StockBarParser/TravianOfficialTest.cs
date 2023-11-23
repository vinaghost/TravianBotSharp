using FluentAssertions;
using MainCore.Parsers.StockBarParser;

namespace TestProject.Parsers.StockBarParser
{
    [TestClass]
    public class TravianOfficialTest : ParserTestBase<TravianOfficial>
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            parts = Helper.GetParts<TravianOfficialTest>();
        }

        [TestMethod]
        public void Get_ShouldBeCorrect()
        {
            var (parser, html) = Setup("TravianOfficial.html");
            var dto = parser.Get(html);
            dto.Wood.Should().Be(173_604);
            dto.Clay.Should().Be(129);
            dto.Iron.Should().Be(255_036);
            dto.Crop.Should().Be(640_000);
            dto.FreeCrop.Should().Be(74_061);
            dto.Warehouse.Should().Be(320_000);
            dto.Granary.Should().Be(640_000);
        }
    }
}