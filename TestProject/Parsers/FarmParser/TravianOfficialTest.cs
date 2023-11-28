using FluentAssertions;
using MainCore.Entities;
using MainCore.Parsers.FarmParser;

namespace TestProject.Parsers.FarmParser
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
        public void GetStartButton_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TravianOfficial.html");

            var node = parser.GetStartButton(html, new FarmId(2008));
            node.Should().NotBeNull();
        }

        [TestMethod]
        public void GetStartAllButton_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TravianOfficial.html");

            var node = parser.GetStartAllButton(html);
            node.Should().NotBeNull();
        }

        [TestMethod]
        public void Get_Count_ShouldBeCorrect()
        {
            var (parser, html) = Setup("TravianOfficial.html");
            var dto = parser.Get(html);

            dto.Count().Should().Be(7);
        }

        [TestMethod]
        public void Get_Content_ShouldBeCorrect()
        {
            var (parser, html) = Setup("TravianOfficial.html");
            var dto = parser.Get(html).FirstOrDefault();

            dto.Id.Should().Be(new FarmId(280));
            dto.Name.Should().Be("D");
        }
    }
}