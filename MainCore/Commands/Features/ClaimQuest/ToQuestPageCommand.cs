using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.ClaimQuest
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class ToQuestPageCommand(DataService dataService) : CommandBase(dataService), ICommand
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
            result = await chromeBrowser.Click(By.XPath(adventure.XPath), "tasks", tableShow, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}