﻿namespace MainCore.Commands.Navigate
{
    [Handler]
    public static partial class ToDorfCommand
    {
        public sealed record Command(int Dorf) : ICommand;

        private static async ValueTask<Result> HandleAsync(
           Command command,
           IChromeBrowser browser,
           CancellationToken cancellationToken
           )
        {
            var dorf = command.Dorf;

            var currentUrl = browser.CurrentUrl;
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

            var button = NavigationBarParser.GetDorfButton(browser.Html, dorf);
            if (button is null) return Retry.ButtonNotFound($"dorf{dorf}");

            Result result;
            result = await browser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result;
            result = await browser.WaitPageChanged($"dorf{dorf}", cancellationToken);
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