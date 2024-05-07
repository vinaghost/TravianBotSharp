namespace MainCore.Commands.Queries
{
    public class CountQueueBuilding
    {
        public int Execute(IChromeBrowser chromeBrowser)
        {
            return GetNodes(chromeBrowser.Html).Count;
        }

        private static List<HtmlNode> GetNodes(HtmlDocument doc)
        {
            var finishButton = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("finishNow"));
            if (finishButton is null) return new();
            return finishButton.ParentNode.Descendants("li").ToList();
        }
    }
}