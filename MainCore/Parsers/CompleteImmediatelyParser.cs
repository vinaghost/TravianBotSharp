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
            var finishClass = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("finishNow"));
            if (finishClass is null) return null;
            var button = finishClass
                .Descendants("button")
                .FirstOrDefault();
            return button;
        }

        public static HtmlNode GetConfirmButton(HtmlDocument doc)
        {
            var dialog = doc.GetElementbyId("finishNowDialog");
            if (dialog is null) return null;
            var button = dialog
                .Descendants("button")
                .FirstOrDefault();
            return button;
        }
    }
}