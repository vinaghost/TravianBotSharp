using MainCore.Commands.Abstract;

namespace MainCore.Commands.Navigate
{
    public class ToDorfCommand : NavigationBarCommand
    {
        public async Task<Result> Execute(IChromeBrowser chromeBrowser, int dorf, bool forceReload, CancellationToken cancellationToken)
        {
            var currentUrl = chromeBrowser.CurrentUrl;
            var currentDorf = GetCurrentDorf(currentUrl);
            if (dorf == 0)
            {
                if (currentDorf == 0) dorf = 1;
                else dorf = currentDorf;
            }

            if (currentDorf != 0 && dorf == currentDorf && !forceReload)
            {
                return Result.Ok();
            }

            var html = chromeBrowser.Html;

            var button = GetDorfButton(html, dorf);
            if (button is null) return Retry.ButtonNotFound($"dorf{dorf}");

            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath), $"dorf{dorf}", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private static int GetCurrentDorf(string url)
        {
            if (url.Contains("dorf1")) return 1;
            if (url.Contains("dorf2")) return 2;
            return 0;
        }

        private static HtmlNode GetDorfButton(HtmlDocument doc, int dorf)
        {
            return dorf switch
            {
                1 => GetResourceButton(doc),
                2 => GetBuildingButton(doc),
                _ => null,
            };
        }

        private static HtmlNode GetResourceButton(HtmlDocument doc) => GetButton(doc, 1);

        private static HtmlNode GetBuildingButton(HtmlDocument doc) => GetButton(doc, 2);
    }
}