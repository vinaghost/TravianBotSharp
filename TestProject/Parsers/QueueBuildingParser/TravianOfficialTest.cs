using FluentAssertions;
using MainCore.Parsers.QueueBuildingParser;

namespace TestProject.Parsers.QueueBuildingParser
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
        public void Get_Count_ShouldBeCorrect()
        {
            var (parser, html) = Setup("TravianOfficial.html");

            var dto = parser.Get(html).ToList();

            dto.Count.Should().Be(4);
        }

        [TestMethod]
        public void Get_Content_ShouldBeCorrect()
        {
            var (parser, html) = Setup("TravianOfficial.html");

            var dto = parser.Get(html).FirstOrDefault();

            dto.Position.Should().Be(0);
            dto.Location.Should().Be(-1);
            dto.Level.Should().Be(13);
            dto.Type.Should().Be("TradeOffice");
        }
    }
}