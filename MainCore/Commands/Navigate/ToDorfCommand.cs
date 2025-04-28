using MainCore.Commands.Abstract;

namespace MainCore.Commands.Navigate
{
    [RegisterScoped<ToDorfCommand>]
    public class ToDorfCommand(IDataService dataService) : CommandBase(dataService), ICommand<int>
    {
        public async Task<Result> Execute(int dorf, CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;

            var currentUrl = chromeBrowser.CurrentUrl;
            var currentDorf = GetCurrentDorf(currentUrl);
            if (dorf == 0)
            {
                if (currentDorf == 0) dorf = 1;
                else dorf = currentDorf;
            }

            if (currentDorf != 0 && dorf == currentDorf)
            {
                return Result.Ok();
            }

            var html = chromeBrowser.Html;

            var button = NavigationBarParser.GetDorfButton(html, dorf);
            if (button is null) return Retry.ButtonNotFound($"dorf{dorf}");

            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await chromeBrowser.WaitPageChanged($"dorf{dorf}", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private static int GetCurrentDorf(string url)
        {
            if (url.Contains("dorf1")) return 1;
            if (url.Contains("dorf2")) return 2;
            return 0;
        }
    }
}