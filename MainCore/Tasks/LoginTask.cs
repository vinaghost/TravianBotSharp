using MainCore.Commands.Features;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class LoginTask(IDelayService delayService,
                                          IChromeBrowser browser,
                                          LoginCommand.Handler loginCommand,
                                          ToDorfCommand.Handler toDorfCommand)
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => "Login";
        }

        private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
        {
            Result result;
            result = await loginCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;

            await delayService.DelayTask(cancellationToken);

            await AccecptCookieConsent();

            result = await DisableContextualHelp();
            if (result.IsFailed) return result;

            result = await toDorfCommand.HandleAsync(new(0), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private async ValueTask AccecptCookieConsent()
        {
            var cmpwrapper = browser.CurrentPage.Locator("div#cmpwrapper");
            if (await cmpwrapper.CountAsync() == 0)
            {
                return;
            }

            var acceptButton = cmpwrapper.Locator(".cmpboxbtn.cmpboxbtnyes.cmptxt_btn_yes");
            if (await acceptButton.CountAsync() == 0)
            {
                return;
            }
            await acceptButton.ClickAsync();
        }

        private async Task<Result> DisableContextualHelp()
        {
            var contextualHelpEnable = await OptionParser.IsContextualHelpEnable(browser.CurrentPage);
            if (!contextualHelpEnable) return Result.Ok();

            var result = await browser.Click(OptionParser.GetOptionButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Click(OptionParser.GetHideContextualHelpOption(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Click(OptionParser.GetSubmitButton(browser.CurrentPage));
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}