namespace MainCore.Test.Parsers
{
    public class NpcResourceParser : BaseParser
    {
        private const string Marketplace = "Parsers/NpcResource/Marketplace.html";
        private const string DistributeDialog = "Parsers/NpcResource/DistributeDialog.html";
        private const string RedeemDialog = "Parsers/NpcResource/RedeemDialog.html";

        [Theory]
        [InlineData(Marketplace, false)]
        [InlineData(DistributeDialog, true)]
        [InlineData(RedeemDialog, true)]
        public void IsNpcDialog(string file, bool expected)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.NpcResourceParser.IsNpcDialog(_html);
            actual.ShouldBe(expected);
        }

        [Fact]
        public void GetExchangeResourcesButton()
        {
            _html.Load(Marketplace);
            var result = MainCore.Parsers.NpcResourceParser.GetExchangeResourcesButton(_html);
            result.ShouldNotBeNull();
        }

        [Fact]
        public void GetRedeemButton()
        {
            _html.Load(RedeemDialog);
            var result = MainCore.Parsers.NpcResourceParser.GetRedeemButton(_html);
            result.ShouldNotBeNull();
        }

        [Fact]
        public void GetSum()
        {
            _html.Load(DistributeDialog);
            var result = MainCore.Parsers.NpcResourceParser.GetSum(_html);
            result.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void GetInputs()
        {
            _html.Load(DistributeDialog);
            var result = MainCore.Parsers.NpcResourceParser.GetInputs(_html);
            result.Count().ShouldBe(4);
        }
    }
}
