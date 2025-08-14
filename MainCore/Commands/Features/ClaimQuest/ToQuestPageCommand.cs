#pragma warning disable S1172

namespace MainCore.Commands.Features.ClaimQuest
{
    [Handler]
    public static partial class ToQuestPageCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {
            var adventure = QuestParser.GetQuestMaster(browser.Html);
            if (adventure is null) return Retry.ButtonNotFound("quest master");

            static bool TableShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return QuestParser.IsQuestPage(doc);
            }

            var result = await browser.Click(By.XPath(adventure.XPath), cancellationToken);
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged("tasks", TableShow, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}