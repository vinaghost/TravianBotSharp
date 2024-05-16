namespace MainCore.Commands.Features.DisableContextualHelp
{
    public class DisableContextualHelpCommand
    {
        public async Task<Result> Execute(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var option = GetHideContextualHelpOption(html);
            if (option is null) return Retry.NotFound("hide contextual help", "option");
            Result result;
            result = await chromeBrowser.Click(By.XPath(option.XPath), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var button = GetSubmitButton(html);
            result = await chromeBrowser.Click(By.XPath(button.XPath), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private static HtmlNode GetHideContextualHelpOption(HtmlDocument doc)
        {
            var node = doc.GetElementbyId("hideContextualHelp");
            return node;
        }

        private static HtmlNode GetSubmitButton(HtmlDocument doc)
        {
            var div = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("submitButtonContainer"));
            if (div is null) return null;
            var button = div.Descendants("button").FirstOrDefault();
            return button;
        }
    }
}