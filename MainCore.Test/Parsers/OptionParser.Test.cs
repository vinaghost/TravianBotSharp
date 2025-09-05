namespace MainCore.Test.Parsers
{
    public class OptionParser : BaseParser
    {
        private const string Buildings = "Parsers/Option/Buildings.html";
        private const string OptionsPage = "Parsers/Option/OptionsPage.html";
        private const string ResourcesWithContextHelp = "Parsers/Option/ResourcesWithContextHelp.html";

        [Theory]
        [InlineData(Buildings, false)]
        [InlineData(ResourcesWithContextHelp, true)]
        public void IsContextualHelpEnable(string file, bool expected)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.OptionParser.IsContextualHelpEnable(_html);
            actual.ShouldBe(expected);
        }

        [Fact]
        public void GetOptionButton()
        {
            _html.Load(OptionsPage);
            var result = MainCore.Parsers.OptionParser.GetOptionButton(_html);
            result.ShouldNotBeNull();
        }

        [Fact]
        public void GetHideContextualHelpOption()
        {
            _html.Load(OptionsPage);
            var result = MainCore.Parsers.OptionParser.GetHideContextualHelpOption(_html);
            result.ShouldNotBeNull();
        }

        [Fact]
        public void GetSubmitButton()
        {
            _html.Load(OptionsPage);
            var result = MainCore.Parsers.OptionParser.GetSubmitButton(_html);
            result.ShouldNotBeNull();
        }
    }
}
