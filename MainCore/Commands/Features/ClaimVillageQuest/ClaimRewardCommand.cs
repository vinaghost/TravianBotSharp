using System;
using System.Collections.Generic;
using System.Text;

using MainCore.Commands.Navigate;

namespace MainCore.Commands.Features.ClaimVillageQuest
{
    [Handler]
    public sealed partial class ClaimRewardCommand(SwitchTabCommand.Handler switchTabCommand)
    {
        public sealed record Command(IChromeBrowser Browser);

        private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var browser = command.Browser;

            Result result;

            do
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Cancel.Error;
                }

                var quest = QuestParser.GetQuestCollectButton(browser.CurrentPage);

                if (await quest.CountAsync() == 0)
                {
                    result = await switchTabCommand.HandleAsync(new(1), cancellationToken);
                    if (result.IsFailed) return result;

                    quest = QuestParser.GetQuestCollectButton(browser.CurrentPage);
                }

                if (await quest.CountAsync() == 0)
                {
                    return Result.Ok();
                }

                result = await browser.Click(quest.First);
                if (result.IsFailed) return result;
            }
            while (await QuestParser.IsQuestClaimable(browser.CurrentPage));

            return Result.Ok();
        }
    }
}