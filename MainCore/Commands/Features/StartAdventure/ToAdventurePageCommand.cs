using MainCore.Commands.Base;

namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public static partial class ToAdventurePageCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeManager chromeManager,
            CancellationToken cancellationToken)
        {
            var chromeBrowser = chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var adventure = AdventureParser.GetHeroAdventureButton(html);
            if (adventure is null) return Retry.ButtonNotFound("hero adventure");

            static bool TableShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return AdventureParser.IsAdventurePage(doc);
            }

            var result = await chromeBrowser.Click(By.XPath(adventure.XPath));
            if (result.IsFailed) return result;

            result = await chromeBrowser.WaitPageChanged("adventures", TableShow, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}