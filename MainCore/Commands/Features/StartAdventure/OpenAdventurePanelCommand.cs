namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public sealed partial class OpenAdventurePanelCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => nameof(OpenAdventurePanelCommand).Humanize();
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            Result result;

            result = await browser.Click(AdventureParser.GetHeroAdventureButton(browser.CurrentPage));
            if (result.IsFailed) return result;
            result = await browser.WaitPageChanged("hero/adventures");
            if (result.IsFailed) return result;
            result = await browser.Wait(AdventureParser.GetAdventureDurationSection(browser.CurrentPage));
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}