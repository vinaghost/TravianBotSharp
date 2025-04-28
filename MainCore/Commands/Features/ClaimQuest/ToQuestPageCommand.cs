using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.ClaimQuest
{
    [RegisterScoped<ToQuestPageCommand>]
    public class ToQuestPageCommand(IDataService dataService) : CommandBase(dataService), ICommand
    {
        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;

            var adventure = QuestParser.GetQuestMaster(chromeBrowser.Html);
            if (adventure is null) return Retry.ButtonNotFound("quest master");

            static bool tableShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return QuestParser.IsQuestPage(doc);
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(adventure.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageChanged("tasks", tableShow, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}