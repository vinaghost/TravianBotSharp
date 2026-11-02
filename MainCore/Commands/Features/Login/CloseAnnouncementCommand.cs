using System;
using System.Collections.Generic;
using System.Text;

namespace MainCore.Commands.Features.Login
{
    [Handler]
    public sealed partial class CloseAnnouncementCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => nameof(CloseAnnouncementCommand).Humanize();
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            Result result;

            var cmpwrapper = browser.CurrentPage.Locator("#constructionPlansSpawnedAnnouncement");
            if (await cmpwrapper.CountAsync() == 0)
            {
                return Result.Ok();
            }

            var acceptButton = cmpwrapper.Locator("#constructionPlansButton");
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