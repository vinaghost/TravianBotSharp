using FluentAssertions;
using MainCore.Parsers.OptionPageParser;

namespace TestProject.Parsers.OptionPageParser
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
        public void IsContextualHelpShow_ShouldBeTrue()
        {
            var (parser, html) = Setup("TravianOfficial.html");

            var value = parser.IsContextualHelpShow(html);
            value.Should().BeTrue();
        }

        [TestMethod]
        public void GetOptionButton_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TravianOfficial.html");

            var value = parser.GetOptionButton(html);
            value.Should().NotBeNull();
        }

        [TestMethod]
        public void GetHideContextualHelpOption_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TravianOfficial_options.html");

            var value = parser.GetHideContextualHelpOption(html);
            value.Should().NotBeNull();
        }

        [TestMethod]
        public void GetReporPerPageOption_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TravianOfficial_options.html");

            var value = parser.GetReporPerPageOption(html);
            value.Should().NotBeNull();
        }

        [TestMethod]
        public void GetMovementsPerPageOption_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TravianOfficial_options.html");

            var value = parser.GetMovementsPerPageOption(html);
            value.Should().NotBeNull();
        }

        [TestMethod]
        public void GetSubmitButton_ShouldNotBeNull()
        {
            var (parser, html) = Setup("TravianOfficial_options.html");

            var value = parser.GetSubmitButton(html);
            value.Should().NotBeNull();
        }
    }
}