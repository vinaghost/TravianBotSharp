namespace MainCore.Test.Parsers
{
    public class QuestParser : BaseParser
    {
        private const string QuestClaimable = "Parsers/Quest/QuestClaimable.html";
        private const string QuestNotClaimable = "Parsers/Quest/QuestNotClaimable.html";
        private const string QuestPage = "Parsers/Quest/QuestPage.html";

        [Theory]
        [InlineData(QuestClaimable)]
        [InlineData(QuestNotClaimable)]
        public void GetQuestMaster(string file)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.QuestParser.GetQuestMaster(_html);
            actual.Should().NotBeNull();
        }

        [Theory]
        [InlineData(QuestClaimable, true)]
        [InlineData(QuestNotClaimable, false)]
        public void IsQuestClaimable(string file, bool expected)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.QuestParser.IsQuestClaimable(_html);
            actual.Should().Be(expected);
        }

        [Fact]
        public void GetQuestCollectButton()
        {
            _html.Load(QuestPage);
            var result = MainCore.Parsers.QuestParser.GetQuestCollectButton(_html);
            result.Should().NotBeNull();
        }

        [Fact]
        public void IsQuestPage()
        {
            _html.Load(QuestPage);
            var result = MainCore.Parsers.QuestParser.IsQuestPage(_html);
            result.Should().BeTrue();
        }
    }
}