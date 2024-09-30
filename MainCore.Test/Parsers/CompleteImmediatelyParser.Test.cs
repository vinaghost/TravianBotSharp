namespace MainCore.Test.Parsers
{
    public class CompleteImmediatelyParser : BaseParser
    {
        private const string BuildingsWithEmptyQueue = "Parsers/CompleteImmediately/BuildingsWithEmptyQueue.html";
        private const string BuildingsWithQueueHasOne = "Parsers/CompleteImmediately/BuildingsWithQueueHasOne.html";
        private const string BuildingsWithQueueHasTwo = "Parsers/CompleteImmediately/BuildingsWithQueueHasTwo.html";
        private const string ConfirmDialog = "Parsers/CompleteImmediately/ConfirmDialog.html";

        [Theory]
        [InlineData(BuildingsWithEmptyQueue, 0)]
        [InlineData(BuildingsWithQueueHasOne, 1)]
        [InlineData(BuildingsWithQueueHasTwo, 2)]
        public void CountQueueBuilding(string path, int expected)
        {
            _html.Load(path);
            var actual = MainCore.Parsers.CompleteImmediatelyParser.CountQueueBuilding(_html);
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(BuildingsWithQueueHasOne)]
        [InlineData(BuildingsWithQueueHasTwo)]
        public void GetCompleteButton(string path)
        {
            _html.Load(path);
            var actual = MainCore.Parsers.CompleteImmediatelyParser.GetCompleteButton(_html);
            actual.Should().NotBeNull();
        }

        [Fact]
        public void GetConfirmButton()
        {
            _html.Load(ConfirmDialog);
            var actual = MainCore.Parsers.CompleteImmediatelyParser.GetConfirmButton(_html);
            actual.Should().NotBeNull();
        }
    }
}