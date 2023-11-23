using FluentAssertions;
using MainCore.Parsers.CompleteImmediatelyParser;

namespace TestProject.Parsers.CompleteImmediatelyParser
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
        public void GetCompleteButton_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TravianOfficial.html");

            var node = parser.GetCompleteButton(html);
            node.Should().NotBeNull();
        }

        [TestMethod]
        public void GetConfirmButton_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TravianOfficial_dialogue.html");

            var node = parser.GetConfirmButton(html);
            node.Should().NotBeNull();
        }
    }
}