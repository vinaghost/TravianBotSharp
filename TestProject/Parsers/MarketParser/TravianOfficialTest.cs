using FluentAssertions;
using MainCore.Parsers.MarketParser;

namespace TestProject.Parsers.MarketParser
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
        public void GetExchangeResourcesButton_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TravianOfficial.html");

            var node = parser.GetExchangeResourcesButton(html);
            node.Should().NotBeNull();
        }

        [TestMethod]
        public void NPCDialogShown_ShouldBeFalse_WhenNormalPage()
        {
            var (parser, html) = Setup("TravianOfficial.html");

            var node = parser.NPCDialogShown(html);
            node.Should().BeFalse();
        }

        [TestMethod]
        public void NPCDialogShown_ShouldBeTrue_WhenDialogOpen()
        {
            var (parser, html) = Setup("TravianOfficial_dialog.html");

            var node = parser.NPCDialogShown(html);
            node.Should().BeTrue();
        }

        [TestMethod]
        public void GetDistributeButton_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TravianOfficial_dialog.html");

            var node = parser.GetDistributeButton(html);
            node.Should().NotBeNull();
        }

        [TestMethod]
        public void GetRedeemButton_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TravianOfficial_dialog.html");

            var node = parser.GetRedeemButton(html);
            node.Should().NotBeNull();
        }

        [TestMethod]
        public void GetSum_ShouldNotBeNegative()
        {
            var (parser, html) = Setup("TravianOfficial_dialog.html");

            var sum = parser.GetSum(html);
            sum.Should().BeGreaterThanOrEqualTo(0);
        }

        [TestMethod]
        public void GetInputs_Count_ShouldBe4()
        {
            var (parser, html) = Setup("TravianOfficial_dialog.html");

            var node = parser.GetInputs(html);
            node.Count().Should().Be(4);
        }
    }
}