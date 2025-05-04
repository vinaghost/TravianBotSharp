using MainCore.Commands.Base;

namespace MainCore.Commands.Checks
{
    [Handler]
    public static partial class CheckQuestCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : ICommand;

        private static async ValueTask HandleAsync(
           Command command,
           IChromeManager chromeManager,
           QuestUpdated.Handler questUpdated,
           CancellationToken cancellationToken
           )
        {
            var (accountId, villageId) = command;
            var browser = chromeManager.Get(accountId);
            if (!QuestParser.IsQuestClaimable(browser.Html)) return;

            await questUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
        }
    }
}