namespace MainCore.Commands.Features.ClaimQuest
{
    [Handler]
    public static partial class ToQuestPageCommand
    {
        public sealed record Command(AccountId AccountId) : ICustomCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeManager chromeManager,
            CancellationToken cancellationToken)
        {
            var chromeBrowser = chromeManager.Get(command.AccountId);

            var adventure = QuestParser.GetQuestMaster(chromeBrowser.Html);
            if (adventure is null) return Retry.ButtonNotFound("quest master");

            static bool TableShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return QuestParser.IsQuestPage(doc);
            }

            var result = await chromeBrowser.Click(By.XPath(adventure.XPath));
            if (result.IsFailed) return result;

            result = await chromeBrowser.WaitPageChanged("tasks", TableShow, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}