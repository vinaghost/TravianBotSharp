using System;
using System.Collections.Generic;
using System.Text;

namespace MainCore.Commands.Features.Login
{
    [Handler]
    public static partial class AccecptCookieConsentCommand
    {
        public sealed record Command(IChromeBrowser Browser);

        private static async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = command.Browser;
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