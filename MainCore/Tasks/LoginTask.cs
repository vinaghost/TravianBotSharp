using MainCore.Commands.Features;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class LoginTask(
        IDelayService delayService,
        IChromeBrowser browser,
        IDbContextFactory<AppDbContext> contextFactory,
        ToDorfCommand.Handler toDorfCommand)
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => "Login";
        }

        private async ValueTask<Result> HandleAsync(Task task)
        {
            await AccecptCookieConsent();

            Result result;
            result = await Login(task.AccountId);
            if (result.IsFailed) return result;

            result = await DisableContextualHelp();
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private async ValueTask<Result> Login(AccountId accountId)
        {
            if (await LoginParser.IsIngamePage(browser.CurrentPage)) return Result.Ok();

            var (username, password) = GetLoginInfo(accountId);

            Result result;

            result = await browser.Input(LoginParser.GetUsernameInput(browser.CurrentPage), username);
            if (result.IsFailed) return result;

            result = await browser.Input(LoginParser.GetPasswordInput(browser.CurrentPage), password);
            if (result.IsFailed) return result;

            result = await browser.Click(LoginParser.GetLoginButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged("dorf");
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

        private async ValueTask<Result> DisableContextualHelp()
        {
            await delayService.DelayTask();

            var contextualHelpEnable = await OptionParser.IsContextualHelpEnable(browser.CurrentPage);
            if (!contextualHelpEnable) return Result.Ok();

            var result = await browser.Click(OptionParser.GetOptionButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Click(OptionParser.GetHideContextualHelpOption(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Click(OptionParser.GetSubmitButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await toDorfCommand.HandleAsync(new(0));
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private (string username, string password) GetLoginInfo(AccountId accountId)
        {
            using var context = contextFactory.CreateDbContext();
            var data = context.Accesses
                .Where(x => x.AccountId == accountId.Value)
                .OrderByDescending(x => x.LastUsed)
                .Select(x => new { x.Username, x.Password })
                .FirstOrDefault();

            if (data is null) return ("", "");

            return (data.Username, data.Password);
        }
    }
}