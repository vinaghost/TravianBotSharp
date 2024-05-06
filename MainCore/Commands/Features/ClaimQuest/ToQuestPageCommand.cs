using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.ClaimQuest
{
    public class ToQuestPageCommand : QuestCommand
    {
        public async Task<Result> Execute(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;

            var adventure = GetQuestMaster(html);
            if (adventure is null) return Retry.ButtonNotFound("quest master");

            Result result;
            result = await chromeBrowser.Click(By.XPath(adventure.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageChanged("tasks", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool tableShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var table = doc.DocumentNode
                    .Descendants("div")
                    .Where(x => x.HasClass("tasks") && x.HasClass("tasksVillage"))
                    .Any();
                return table;
            };

            result = await chromeBrowser.Wait(tableShow, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}