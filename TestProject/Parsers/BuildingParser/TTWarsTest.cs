using MainCore.Parsers.BuildingParser;

namespace TestProject.Parsers.BuildingParser
{
    [TestClass]
    public class TTWarsTest : ParserTestBase<TTWars>
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            parts = Helper.GetParts<TTWarsTest>();
        }

        //[DataTestMethod]
        //[DataRow(1, "TTWars_dorf1.html")]
        //[DataRow(39, "TTWars_dorf2.html")]
        //public void GetBuilding_ShouldNotBeNull(int location, string fileName)
        //{
        //    var (parser, html) = Setup(fileName);

        //    var node = parser.GetBuilding(html, location);
        //    node.Should().NotBeNull();
        //}

        //[DataTestMethod]
        //[DataRow(39, "TTWars_dorf1.html")]
        //[DataRow(1, "TTWars_dorf2.html")]
        //public void GetBuilding_ShouldBeNull(int location, string fileName)
        //{
        //    var (parser, html) = Setup(fileName);

        //    var node = parser.GetBuilding(html, location);
        //    node.Should().BeNull();
        //}
    }
}