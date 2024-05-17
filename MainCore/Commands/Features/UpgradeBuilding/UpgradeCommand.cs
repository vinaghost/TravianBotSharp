namespace MainCore.Commands.Features.UpgradeBuilding
{
    public class UpgradeCommand
    {
        public async Task<Result> Execute(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;

            var button = GetUpgradeButton(html);
            if (button is null) return Retry.ButtonNotFound("upgrade");

            var result = await chromeBrowser.Click(By.XPath(button.XPath), "dorf", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private static HtmlNode GetUpgradeButton(HtmlDocument doc)
        {
            var node = doc.DocumentNode.Descendants("div")
               .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            if (node is null) return null;

            var button = node.Descendants("button")
                .FirstOrDefault(x => x.HasClass("build"));

            return button;
        }
    }
}