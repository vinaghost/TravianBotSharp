using MainCore.Commands.Base;

namespace MainCore.Commands.Features.DisableContextualHelp
{
    [Handler]
    public static partial class DisableContextualHelpCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeManager chromeManager,
            CancellationToken cancellationToken)
        {
            var chromeBrowser = chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var option = OptionParser.GetHideContextualHelpOption(html);
            if (option is null) return Retry.NotFound("hide contextual help", "option");

            var result = await chromeBrowser.Click(By.XPath(option.XPath));
            if (result.IsFailed) return result;

            var button = OptionParser.GetSubmitButton(html);
            if (button is null) return Retry.ButtonNotFound("submit");

            result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}