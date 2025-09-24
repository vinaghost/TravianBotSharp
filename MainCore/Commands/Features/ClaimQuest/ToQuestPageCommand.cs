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
            var (_, isFailed, element, errors) = await browser.GetElement(doc => QuestParser.GetQuestMaster(doc), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            var result = await browser.Click(element, cancellationToken);
            if (result.IsFailed) return result;

            static bool TableShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return QuestParser.IsQuestPage(doc);
            }
            result = await browser.Wait(TableShow, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}