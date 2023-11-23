using FluentAssertions;
using MainCore.Common.Enums;
using MainCore.Parsers.InfrastructureParser;

namespace TestProject.Parsers.InfrastructureParser
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

            var dto = parser.Get(html);

            dto.Count().Should().Be(22);
        }

        [TestMethod]
        public void Get_Content_ShouldBeCorrect()
        {
            var (parser, html) = Setup("TravianOfficial.html");

            var dto = parser.Get(html).FirstOrDefault();

            dto.Type.Should().Be(BuildingEnums.Granary);
            dto.Location.Should().Be(19);
            dto.Level.Should().Be(20);
            dto.IsUnderConstruction.Should().Be(false);
        }
    }
}