using FluentAssertions;
using MainCore.Parsers.CompleteImmediatelyParser;

namespace TestProject.Parsers.CompleteImmediatelyParser
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
        public void GetCompleteButton_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TTWars.html");

            var node = parser.GetCompleteButton(html);
            node.Should().NotBeNull();
        }

        [TestMethod]
        public void GetConfirmButton_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TTWars_dialogue.html");

            var node = parser.GetConfirmButton(html);
            node.Should().NotBeNull();
        }
    }
}