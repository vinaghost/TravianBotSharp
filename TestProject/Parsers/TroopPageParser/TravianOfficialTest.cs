using MainCore.Parsers.TroopPageParser;

namespace TestProject.Parsers.TroopPageParser
{
    [TestClass]
    public class TravianOfficialTest : ParserTestBase<TravianOfficial>
    {
        private const string BARRACK = "barrack";
        private const string STABLE = "stable";
        private const string WORKSHOP = "workshop";

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            parts = Helper.GetParts<TravianOfficialTest>();
        }

        //[DataTestMethod]
        //[DataRow(BARRACK, TroopEnums.Phalanx)]
        //[DataRow(STABLE, TroopEnums.Pathfinder)]
        //[DataRow(WORKSHOP, TroopEnums.GaulCatapult)]
        //public void GetNode_ShouldNotBeNull(string type, TroopEnums troop)
        //{
        //    var (parser, html) = Setup($"TravianOfficial_{type}.html");

        //    var node = parser.GetNode(html, troop);
        //    node.Should().NotBeNull();
        //}

        //[DataTestMethod]
        //[DataRow(BARRACK, TroopEnums.TeutonSettler)]
        //[DataRow(STABLE, TroopEnums.TeutonSettler)]
        //[DataRow(WORKSHOP, TroopEnums.TeutonSettler)]
        //public void GetNode_ShouldBeNull(string type, TroopEnums troop)
        //{
        //    var (parser, html) = Setup($"TravianOfficial_{type}.html");

        //    var node = parser.GetNode(html, troop);
        //    node.Should().BeNull();
        //}
    }
}