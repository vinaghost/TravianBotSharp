using FluentAssertions;
using MainCore.Parsers.QueueBuildingParser;

namespace TestProject.Parsers.QueueBuildingParser
{
    [TestClass]
    public class TTWarsTest : ParserTestBase<TTWars>
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            parts = Helper.GetParts<TTWarsTest>();
        }

        [TestMethod]
        public void Get_ShouldBeCorrect()
        {
            var (parser, html) = Setup("TTWars.html");

            var dto = parser.Get(html).ToList();

            dto.Count.Should().Be(4);
        }

        [TestMethod]
        public void Get_Content_ShouldBeCorrect()
        {
            var (parser, html) = Setup("TTWars.html");

            var dto = parser.Get(html).FirstOrDefault();

            dto.Position.Should().Be(0);
            dto.Location.Should().Be(-1);
            dto.Level.Should().Be(8);
            dto.Type.Should().Be("MainBuilding");
        }
    }
}