using System;
using System.Collections.Generic;
using System.Text;

using MainCore.Commands.Navigate;

namespace MainCore.Commands.Features.SetHeroPoint
{
    [Handler]
    public sealed partial class OpenSetPointPanelCommand(SwitchTabCommand.Handler switchTabCommand)
    {
        public sealed record Command(IChromeBrowser Browser);

        private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var browser = command.Browser;
            Result result;

            var heroAvatar = browser.CurrentPage.Locator("#topBarHero #heroImageButton");
            result = await browser.Click(heroAvatar);
            if (result.IsFailed) return result;

            var heroPageLocator = browser.CurrentPage.Locator("#heroV2");
            result = await browser.Wait(heroPageLocator);
            if (result.IsFailed) return result;

            result = await switchTabCommand.HandleAsync(new(1), cancellationToken);
            if (result.IsFailed) return result;

            var attributeBox = browser.CurrentPage.Locator("#heroV2 .attributeBox .heroAttributes .attributes.formV2");
            result = await browser.Wait(attributeBox);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}