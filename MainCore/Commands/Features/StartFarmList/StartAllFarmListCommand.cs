namespace MainCore.Commands.Features.StartFarmList
{
    public class StartAllFarmListCommand
    {
        public async Task<Result> Execute(IChromeBrowser chromeBrowser)
        {
            var html = chromeBrowser.Html;
            var startAllButton = GetStartAllButton(html);
            if (startAllButton is null) return Retry.ButtonNotFound("Start all farms");

            Result result;
            result = await chromeBrowser.Click(By.XPath(startAllButton.XPath), CancellationToken.None);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private static HtmlNode GetStartAllButton(HtmlDocument doc)
        {
            var raidList = doc.GetElementbyId("rallyPointFarmList");
            if (raidList is null) return null;
            var startAll = raidList
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("startAllFarmLists"));
            return startAll;
        }
    }
}