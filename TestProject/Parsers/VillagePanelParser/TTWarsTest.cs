using FluentAssertions;
using MainCore.Entities;
using MainCore.Parsers.VillagePanelParser;

namespace TestProject.Parsers.VillagePanelParser
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

            dto.Count().Should().Be(1);
        }

        [TestMethod]
        public void Get_Content_ShouldBeCorrect()
        {
            var (parser, html) = Setup("TTWars.html");

            var dto = parser.Get(html).FirstOrDefault();

            dto.Id.Should().Be(new VillageId(255147));
            dto.Name.Should().Be("vinaghost`s village");
            dto.X.Should().Be(28);
            dto.Y.Should().Be(82);
            dto.IsActive.Should().Be(true);
            dto.IsUnderAttack.Should().Be(false);
        }

        [DataTestMethod]
        [DataRow(255_147)]
        public void GetVillageNode_ShouldNotBeNull(int villageId)
        {
            var (parser, html) = Setup("TTWars.html");

            var node = parser.GetVillageNode(html, new VillageId(villageId));
            node.Should().NotBeNull();
        }

        [DataTestMethod]
        [DataRow(255147, true)]
        public void IsActive_ShouldNotBeNull(int villageId, bool expected)
        {
            var (parser, html) = Setup("TTWars.html");

            var node = parser.GetVillageNode(html, new VillageId(villageId));
            var result = parser.IsActive(node);
            result.Should().Be(expected);
        }
    }
}