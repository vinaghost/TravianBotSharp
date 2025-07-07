namespace MainCore.Commands.Features.DisableContextualHelp
{
    [Handler]
    public static partial class ToOptionsPageCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command _,
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