using FluentAssertions;
using MainCore.Common.Enums;
using MainCore.Parsers.InfrastructureParser;

namespace TestProject.Parsers.InfrastructureParser
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
        public void Get_Count_ShouldBeCorrect()
        {
            var (parser, html) = Setup("TTWars.html");

            var dto = parser.Get(html);

            dto.Count().Should().Be(22);
        }

        [TestMethod]
        public void Get_Content_ShouldBeCorrect()
        {
            var (parser, html) = Setup("TTWars.html");

            var dto = parser.Get(html).FirstOrDefault();

            dto.Type.Should().Be(BuildingEnums.Site);
            dto.Location.Should().Be(19);
            dto.Level.Should().Be(-1);
            dto.IsUnderConstruction.Should().Be(false);
        }
    }
}