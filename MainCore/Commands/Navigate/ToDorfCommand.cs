namespace MainCore.Commands.Navigate
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

            // ensure the *current* page is fully rendered before looking for the button.
            // `WaitPageChanged` takes a URL fragment but we only care about the logo being
            // visible; passing an empty string makes the URL check no‑op and still waits
            // for the logo, avoiding deadlocks when we're on a non-dorf page.
            var waitResult = await browser.WaitPageChanged("", cancellationToken);
            if (waitResult.IsFailed) return waitResult;

            var (_, isFailed, element, errors) = await browser.GetElement(doc => NavigationBarParser.GetDorfButton(doc, dorf), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            Result result;
            result = await browser.Click(element, cancellationToken);
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged($"dorf{dorf}.php", cancellationToken);
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