namespace MainCore.Commands.Features.ClaimVillageQuest
{
    [Handler]
    public sealed partial class OpenVillageQuestPageCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => nameof(OpenVillageQuestPageCommand).Humanize();
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
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