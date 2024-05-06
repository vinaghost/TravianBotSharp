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
            result = await chromeBrowser.Click(By.XPath(option.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var button = GetSubmitButton(html);
            result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private static HtmlNode GetHideContextualHelpOption(HtmlDocument doc)
        {
            var node = doc.GetElementbyId("hideContextualHelp");
            return node;
        }

        private static HtmlNode GetReporPerPageOption(HtmlDocument doc)
        {
            var node = doc.GetElementbyId("epp");
            return node;
        }

        private static HtmlNode GetMovementsPerPageOption(HtmlDocument doc)
        {
            var node = doc.GetElementbyId("troopMovementsPerPage");
            return node;
        }

        private static HtmlNode GetSubmitButton(HtmlDocument doc)
        {
            var div = doc.DocumentNode.Descendants("div").Where(x => x.HasClass("submitButtonContainer")).FirstOrDefault();
            if (div is null) return null;
            var button = div.Descendants("button").FirstOrDefault();
            return button;
        }
    }
}