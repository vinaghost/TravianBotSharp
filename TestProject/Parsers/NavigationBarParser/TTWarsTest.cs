using MainCore.Parsers.NavigationBarParser;

namespace TestProject.Parsers.NavigationBarParser
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
        public void GetBuildingButton_Vailidate_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TTWars.html");

            var node = parser.GetBuildingButton(html);
            Assert.IsNotNull(node);
        }

        [TestMethod]
        public void GetDailyButton_Vailidate_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TTWars.html");

            var node = parser.GetDailyButton(html);
            Assert.IsNotNull(node);
        }

        [TestMethod]
        public void GetMapButton_Vailidate_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TTWars.html");

            var node = parser.GetMapButton(html);
            Assert.IsNotNull(node);
        }

        [TestMethod]
        public void GetMessageButton_Vailidate_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TTWars.html");

            var node = parser.GetMessageButton(html);
            Assert.IsNotNull(node);
        }

        [TestMethod]
        public void GetReportsButton_Vailidate_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TTWars.html");

            var node = parser.GetReportsButton(html);
            Assert.IsNotNull(node);
        }

        [TestMethod]
        public void GetResourceButton_Vailidate_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TTWars.html");

            var node = parser.GetResourceButton(html);
            Assert.IsNotNull(node);
        }

        [TestMethod]
        public void GetStatisticsButton_Vailidate_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TTWars.html");

            var node = parser.GetStatisticsButton(html);
            Assert.IsNotNull(node);
        }
    }
}