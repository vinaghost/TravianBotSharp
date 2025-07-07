#pragma warning disable S1172

namespace MainCore.Commands.Features.DisableContextualHelp
{
    [Handler]
    public static partial class ToOptionsPageCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser
            )
        {
            var html = browser.Html;

            var button = OptionParser.GetOptionButton(html);
            if (button is null) return Retry.ButtonNotFound("options");

            var result = await browser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}