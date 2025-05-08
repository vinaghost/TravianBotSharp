using MainCore.Constraints;

namespace MainCore.Commands.Features.DisableContextualHelp
{
    [Handler]
    public static partial class DisableContextualHelpCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {

            var html = browser.Html;

            var option = OptionParser.GetHideContextualHelpOption(html);
            if (option is null) return Retry.NotFound("hide contextual help", "option");

            var result = await browser.Click(By.XPath(option.XPath));
            if (result.IsFailed) return result;

            var button = OptionParser.GetSubmitButton(html);
            if (button is null) return Retry.ButtonNotFound("submit");

            result = await browser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}