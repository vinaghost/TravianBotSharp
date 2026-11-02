namespace MainCore.Commands.Features.Login
{
    [Handler]
    public sealed partial class AccecptCookieConsentCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => nameof(AccecptCookieConsentCommand);
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            Result result;

            var cmpwrapper = browser.CurrentPage.Locator("div#cmpwrapper");
            if (await cmpwrapper.CountAsync() == 0)
            {
                return Result.Ok();
            }

            var acceptButton = cmpwrapper.Locator(".cmpboxbtn.cmpboxbtnyes.cmptxt_btn_yes");
            if (await acceptButton.CountAsync() == 0)
            {
                return Result.Ok();
            }
            result = await browser.Click(acceptButton);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}