using MainCore.Commands.Base;

namespace MainCore.Commands.Checks
{
    [Handler]
    public static partial class CheckAdventureCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask HandleAsync(
           Command command,
           IChromeManager chromeManager,
           AdventureUpdated.Handler adventureUpdated,
           CancellationToken cancellationToken
           )
        {
            var accountId = command.AccountId;
            var browser = chromeManager.Get(accountId);
            if (!AdventureParser.CanStartAdventure(browser.Html)) return;

            await adventureUpdated.HandleAsync(new(accountId), cancellationToken);
        }
    }
}