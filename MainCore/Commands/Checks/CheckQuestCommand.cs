using MainCore.Constraints;
using MainCore.Notifications.Handlers.Trigger;

namespace MainCore.Commands.Checks
{
    [Handler]
    public static partial class CheckQuestCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeBrowser browser,
            ClaimQuestTaskTrigger.Handler claimQuestTaskTrigger,
            CancellationToken cancellationToken
           )
        {
            if (!QuestParser.IsQuestClaimable(browser.Html)) return;
            await claimQuestTaskTrigger.HandleAsync(command, cancellationToken);
        }
    }
}