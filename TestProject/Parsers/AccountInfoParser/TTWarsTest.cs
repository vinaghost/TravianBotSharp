using FluentAssertions;
using MainCore.Parsers.AccountInfoParser;

namespace TestProject.Parsers.AccountInfoParser
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

            var dto = parser.Get(html);

            dto.Gold.Should().Be(210);
            dto.Silver.Should().Be(154);
            dto.HasPlusAccount.Should().Be(false);
        }
    }
}