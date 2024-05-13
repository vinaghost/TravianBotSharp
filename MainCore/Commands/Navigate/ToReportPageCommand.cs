using MainCore.Commands.Abstract;

namespace MainCore.Commands.Navigate
{
    public class ToReportPageCommand : NavigationBarCommand
    {
        public async Task<Result> Execute(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;

            var button = GetReportButton(html);
            if (button is null) return Retry.ButtonNotFound($"report");

            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await chromeBrowser.WaitPageChanged($"report", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private static HtmlNode GetReportButton(HtmlDocument doc) => GetButton(doc, 5);
    }
}