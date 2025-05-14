using MainCore.Constraints;
using MainCore.Notifications.Handlers.Trigger;

namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateQuestCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

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