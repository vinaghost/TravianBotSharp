namespace MainCore.Parsers
{
    public static class CompleteImmediatelyParser
    {
        public static int CountQueueBuilding(HtmlDocument doc)
        {
            var finishButton = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("finishNow"));
            if (finishButton is null) return 0;
            var nodes = finishButton.ParentNode
                .Descendants("li");
            return nodes.Count();
        }

        public static HtmlNode GetCompleteButton(HtmlDocument doc)
        {
            var finishDiv = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("finishNow"));

            BrokenParserException.ThrowIfNull(finishDiv);

            var finishButton = finishDiv
                .Descendants("button")
                .FirstOrDefault();
            BrokenParserException.ThrowIfNull(finishButton);
            return finishButton;
        }

        public static HtmlNode GetConfirmButton(HtmlDocument doc)
        {
            var finishDialog = doc.GetElementbyId("finishNowDialog");
            BrokenParserException.ThrowIfNull(finishDialog);

            var confirmFinishbutton = finishDialog
                .Descendants("button")
                .FirstOrDefault();
            BrokenParserException.ThrowIfNull(confirmFinishbutton);
            return confirmFinishbutton;
        }
    }
}