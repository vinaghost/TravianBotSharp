namespace MainCore.Commands.Features.ClaimVillageQuest
{
    [Handler]
    public static partial class OpenVillageQuestPageCommand
    {
        public sealed record Command(IChromeBrowser Browser);

        private static async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = command.Browser;
            Result result;

            var questMaster = QuestParser.GetQuestMaster(browser.CurrentPage);
            result = await browser.Click(questMaster);
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged("tasks");
            if (result.IsFailed) return result;

            var villageQuestPanelReadyLocator = browser.CurrentPage.Locator("#tasks .taskOverview .task .taskImage");
            result = await browser.Wait(villageQuestPanelReadyLocator.First);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}