using MainCore.Constraints;

namespace MainCore.Commands.Checks
{
    [Handler]
    public static partial class CheckQuestCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : ICommand;

        private static async ValueTask HandleAsync(
           Command command,
           IChromeBrowser browser,
           QuestUpdated.Handler questUpdated,
           CancellationToken cancellationToken
           )
        {
            var (accountId, villageId) = command;

            if (!QuestParser.IsQuestClaimable(browser.Html)) return;

            await questUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
        }
    }
}