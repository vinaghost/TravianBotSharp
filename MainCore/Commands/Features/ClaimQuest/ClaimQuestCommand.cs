namespace MainCore.Commands.Features.ClaimQuest
{
    [Handler]
    public static partial class ClaimQuestCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : ICustomCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeManager chromeManager,
            StorageUpdated.Handler storageUpdate, SwitchTabCommand.Handler switchTabCommand, DelayClickCommand.Handler delayClickCommand,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId) = command;
            var browser = chromeManager.Get(accountId);
            HtmlDocument html;
            Result result;

            do
            {
                if (cancellationToken.IsCancellationRequested) return;
                html = browser.Html;
                var quest = QuestParser.GetQuestCollectButton(html);

                if (quest is null)
                {
                    result = await switchTabCommand.HandleAsync(new(accountId, 1), cancellationToken);
                    if (result.IsFailed) return;

                    await delayClickCommand.HandleAsync(new(accountId), cancellationToken);

                    quest = QuestParser.GetQuestCollectButton(browser.Html);
                    if (quest is null) return;

                    result = await browser.Click(By.XPath(quest.XPath));
                    await storageUpdate.HandleAsync(new(accountId, villageId), cancellationToken);
                    return;
                }

                result = await browser.Click(By.XPath(quest.XPath));
                if (result.IsFailed) break;
                await delayClickCommand.HandleAsync(new(accountId), cancellationToken);
            }
            while (QuestParser.IsQuestClaimable(html));

            await storageUpdate.HandleAsync(new(accountId, villageId), cancellationToken);
        }
    }
}