using System;
using System.Collections.Generic;
using System.Text;

namespace MainCore.Commands.Features
{
    [Handler]
    public static partial class InstantUpgradeCommand
    {
        public sealed record Command(IChromeBrowser Browser);

        private static async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = command.Browser;
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