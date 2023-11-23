using FluentAssertions;
using MainCore.Common.Enums;
using MainCore.Parsers.FieldParser;

namespace TestProject.Parsers.FieldParser
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

            dto.Count().Should().Be(18);
        }

        [TestMethod]
        public void Get_Content_ShouldBeCorrect()
        {
            var (parser, html) = Setup("TTWars.html");

            var dto = parser.Get(html).FirstOrDefault();

            dto.Type.Should().Be(BuildingEnums.Woodcutter);
            dto.Location.Should().Be(1);
            dto.Level.Should().Be(10);
            dto.IsUnderConstruction.Should().Be(false);
        }
    }
}