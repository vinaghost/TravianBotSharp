namespace MainCore.Test.Parsers
{
    public class NavigationBarParser : BaseParser
    {
        private const string Buildings = "Parsers/NavigationBar/Buildings.html";

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void GetDorfButton(int input)
        {
            _html.Load(Buildings);
            var actual = MainCore.Parsers.NavigationBarParser.GetDorfButton(_html, input);
            actual.Should().NotBeNull();
        }
    }
}