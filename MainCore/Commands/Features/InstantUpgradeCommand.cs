namespace MainCore.Commands.Features
{
    [Handler]
    public sealed partial class InstantUpgradeCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => $"{nameof(InstantUpgradeCommand)}";
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            Result result;

            var oldQueueCount = await BuildingLayoutParser.CountQueueBuilding(browser.CurrentPage);
            if (oldQueueCount == 0) return Result.Ok();

            var completeButton = CompleteImmediatelyParser.GetCompleteButton(browser.CurrentPage);
            result = await browser.Click(completeButton);
            if (result.IsFailed) return result;

            var confirmButton = CompleteImmediatelyParser.GetConfirmButton(browser.CurrentPage);
            result = await browser.Click(confirmButton);
            if (result.IsFailed) return result;

            var pageBody = browser.CurrentPage.Locator("body");
            result = await browser.Wait(pageBody, condition: "node => node.querySelector('div.buildingList') === null");
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}