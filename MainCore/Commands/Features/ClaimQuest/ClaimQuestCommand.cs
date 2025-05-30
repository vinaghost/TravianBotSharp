﻿using MainCore.Constraints;

namespace MainCore.Commands.Features.ClaimQuest
{
    [Handler]
    public static partial class ClaimQuestCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            SwitchTabCommand.Handler switchTabCommand, DelayClickCommand.Handler delayClickCommand,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId) = command;

            HtmlDocument html;
            Result result;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                html = browser.Html;
                var quest = QuestParser.GetQuestCollectButton(html);

                if (quest is null)
                {
                    result = await switchTabCommand.HandleAsync(new(accountId, 1), cancellationToken);
                    if (result.IsFailed) return result;

                    await delayClickCommand.HandleAsync(new(accountId), cancellationToken);

                    quest = QuestParser.GetQuestCollectButton(browser.Html);
                    if (quest is null) return Result.Ok();

                    result = await browser.Click(By.XPath(quest.XPath));
                    continue;
                }

                result = await browser.Click(By.XPath(quest.XPath));
                if (result.IsFailed) return result;
                await delayClickCommand.HandleAsync(new(accountId), cancellationToken);
            }
            while (QuestParser.IsQuestClaimable(html));

            return Result.Ok();
        }
    }
}