using MainCore.Commands.Base;

namespace MainCore.Commands.Features.DisableContextualHelp
{
    [Handler]
    public static partial class ToOptionsPageCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeManager chromeManager,
            CancellationToken cancellationToken)
        {
            var chromeBrowser = chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var button = OptionParser.GetOptionButton(html);
            if (button is null) return Retry.ButtonNotFound("options");

            var result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}