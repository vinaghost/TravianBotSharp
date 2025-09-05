namespace MainCore.Test.Parsers
{
    public class InfoParser : BaseParser
    {
        private const string NoPlusAccount = "Parsers/Info/NoPlusAccount.html";
        private const string PlusAccount = "Parsers/Info/PlusAccount.html";

        [Fact]
        public void GetGold()
        {
            _html.Load(NoPlusAccount);
            var actual = MainCore.Parsers.InfoParser.GetGold(_html);
            actual.ShouldBeGreaterThan(-1);
        }

        [Fact]
        public void GetSilver()
        {
            _html.Load(NoPlusAccount);
            var actual = MainCore.Parsers.InfoParser.GetSilver(_html);
            actual.ShouldBeGreaterThan(-1);
        }

        [Theory]
        [InlineData(NoPlusAccount, false)]
        [InlineData(PlusAccount, true)]
        public void HasPlusAccount(string file, bool expected)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.InfoParser.HasPlusAccount(_html);
            actual.ShouldBe(expected);
        }
    }
}
