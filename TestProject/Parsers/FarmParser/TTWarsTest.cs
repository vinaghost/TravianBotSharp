using FluentAssertions;
using MainCore.Entities;
using MainCore.Parsers.FarmParser;

namespace TestProject.Parsers.FarmParser
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
        public void GetStartButton_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TTWars.html");

            var node = parser.GetStartButton(html, new FarmId(54));
            node.Should().NotBeNull();
        }

        [TestMethod]
        public void GetStartAllButton_ShouldBeNull()
        {
            var (parser, html) = Setup("TTWars.html");

            var node = parser.GetStartAllButton(html);
            node.Should().BeNull();
        }

        [TestMethod]
        public void Get_Count_ShouldBeCorrect()
        {
            var (parser, html) = Setup("TTWars.html");

            var dto = parser.Get(html);

            dto.Count().Should().Be(5);
        }

        [TestMethod]
        public void Get_Content_ShouldBeCorrect()
        {
            var (parser, html) = Setup("TTWars.html");

            var dto = parser.Get(html).FirstOrDefault();

            dto.Id.Should().Be(new FarmId(54));
            dto.Name.Should().Be("1");
        }
    }
}