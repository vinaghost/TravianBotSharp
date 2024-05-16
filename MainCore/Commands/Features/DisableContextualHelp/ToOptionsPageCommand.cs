namespace MainCore.Commands.Features.DisableContextualHelp
{
    public class ToOptionsPageCommand
    {
        public async Task<Result> Execute(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;

            var button = GetOptionButton(html);
            if (button is null) return Retry.ButtonNotFound("options");

            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private static HtmlNode GetOptionButton(HtmlDocument doc)
        {
            var outOfGame = doc.GetElementbyId("outOfGame");
            if (outOfGame is null) return null;
            var a = outOfGame.Descendants("a").FirstOrDefault(x => x.HasClass("options"));
            return a;
        }
    }
}