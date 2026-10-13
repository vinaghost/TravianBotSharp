namespace MainCore.Commands.Navigate
{
    [Handler]
    public sealed partial class ToDorfCommand(IChromeBrowser browser)
    {
        public sealed record Command(int Dorf) : ICommand;

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var dorf = command.Dorf;

            var currentUrl = browser.CurrentUrl;
            var currentDorf = GetCurrentDorf(currentUrl);
            if (dorf == 0)
            {
                if (currentDorf == 0) dorf = 2;
                else dorf = currentDorf;
            }

            if (currentDorf != 0 && dorf == currentDorf)
            {
                return Result.Ok();
            }

            Result result;
            result = await browser.Click(NavigationBarParser.GetDorfButton(browser.CurrentPage, dorf));
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged($"dorf{dorf}.php");
            if (result.IsFailed) return result;

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