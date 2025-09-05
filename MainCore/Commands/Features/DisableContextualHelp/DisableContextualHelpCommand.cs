#pragma warning disable S1172

namespace MainCore.Commands.Features.DisableContextualHelp
{
    [Handler]
    public static partial class DisableContextualHelpCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IBrowser browser
            )
        {
            var option = OptionParser.GetHideContextualHelpOption(browser.Html);
            if (option is null) return Retry.NotFound("hide contextual help", "option");

            var result = await browser.Click(By.XPath(option.XPath));
            if (result.IsFailed) return result;

            var button = OptionParser.GetSubmitButton(browser.Html);
            if (button is null) return Retry.ButtonNotFound("submit");

            result = await browser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}
