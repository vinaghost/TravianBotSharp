using FluentAssertions;
using MainCore.Entities;
using MainCore.Parsers.VillagePanelParser;

namespace TestProject.Parsers.VillagePanelParser
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

            dto.Count().Should().Be(15);
        }

        [TestMethod]
        public void Get_Content_ShouldBeCorrect()
        {
            var (parser, html) = Setup("TravianOfficial.html");

            var dto = parser.Get(html).FirstOrDefault();

            dto.Id.Should().Be(new VillageId(19501));
            dto.Name.Should().Be("VinaVillage");
            dto.X.Should().Be(114);
            dto.Y.Should().Be(-32);
            dto.IsActive.Should().Be(false);
            dto.IsUnderAttack.Should().Be(false);
        }

        [DataTestMethod]
        [DataRow(19501)]
        [DataRow(21180)]
        public void GetVillageNode_ShouldNotBeNull(int villageId)
        {
            var (parser, html) = Setup("TravianOfficial.html");

            var node = parser.GetVillageNode(html, new VillageId(villageId));
            node.Should().NotBeNull();
        }

        [DataTestMethod]
        [DataRow(23291, true)]
        [DataRow(21180, false)]
        public void IsActive_ShouldBeCorrect(int villageId, bool expected)
        {
            var (parser, html) = Setup("TravianOfficial.html");

            var node = parser.GetVillageNode(html, new VillageId(villageId));
            var result = parser.IsActive(node);
            Assert.AreEqual(expected, result);
        }
    }
}