using FluentAssertions;
using MainCore.Common.Enums;
using MainCore.Parsers.UpgradeBuildingParser;

namespace TestProject.Parsers.UpgradeBuildingParser
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
        public void GetRequiredResource_ShouldBePositive_WhenConstruct()
        {
            var (parser, html) = Setup("TTWars_construct.html");
            var resources = parser.GetRequiredResource(html, true, BuildingEnums.Cranny);

            resources.Should().AllSatisfy(x => x.Should().NotBe(-1L));
        }

        [TestMethod]
        public void GetRequiredResource_ShouldBePositive_WhenUpgrade()
        {
            var (parser, html) = Setup("TTWars_upgrade.html");
            var resources = parser.GetRequiredResource(html, false);

            resources.Should().AllSatisfy(x => x.Should().NotBe(-1L));
        }

        [TestMethod]
        public void GetTimeWhenEnoughResource_ShouldBeGreaterThanZero()
        {
            var (parser, html) = Setup("TTWars_resource.html");
            var time = parser.GetTimeWhenEnoughResource(html, false);

            time.Should().BeGreaterThan(TimeSpan.Zero);
        }

        [TestMethod]
        public void GetConstructButton_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TTWars_construct.html");
            var button = parser.GetConstructButton(html, BuildingEnums.Cranny);

            button.Should().NotBeNull();
        }

        [TestMethod]
        public void GetUpgradeButton_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TTWars_upgrade.html");
            var button = parser.GetUpgradeButton(html);

            button.Should().NotBeNull();
        }

        [TestMethod]
        public void GetSpecialUpgradeButton_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TTWars_upgrade.html");
            var button = parser.GetSpecialUpgradeButton(html);

            button.Should().NotBeNull();
        }
    }
}