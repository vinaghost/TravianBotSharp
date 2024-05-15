namespace MainCore.Commands.Misc
{
    public class MarkAllAsReadCommand
    {
        public async Task<Result> Execute(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var button = html.DocumentNode
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("markAsRead"));

            if (button is null) return Retry.ButtonNotFound("mark as read");

            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await Task.Delay(500);

            result = await chromeBrowser.WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}