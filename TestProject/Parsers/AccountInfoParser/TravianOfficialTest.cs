using FluentAssertions;
using MainCore.Parsers.AccountInfoParser;

namespace TestProject.Parsers.AccountInfoParser
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

            dto.Gold.Should().Be(326);
            dto.Silver.Should().Be(2_060);
            dto.HasPlusAccount.Should().Be(true);
        }
    }
}