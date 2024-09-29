using MainCore.Commands.Abstract;

namespace MainCore.Commands.Navigate
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class ToDorfCommand(DataService dataService) : CommandBase<int>(dataService)
    {
        public override async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;

            var currentUrl = chromeBrowser.CurrentUrl;
            var currentDorf = GetCurrentDorf(currentUrl);
            if (Data == 0)
            {
                if (currentDorf == 0) Data = 1;
                else Data = currentDorf;
            }

            if (currentDorf != 0 && Data == currentDorf)
            {
                return Result.Ok();
            }

            var html = chromeBrowser.Html;

            var button = NavigationBarParser.GetDorfButton(html, Data);
            if (button is null) return Retry.ButtonNotFound($"dorf{Data}");

            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath), $"dorf{Data}", cancellationToken);
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