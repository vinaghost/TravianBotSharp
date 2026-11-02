namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public sealed partial class CheckHeroReadyCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => $"{nameof(CheckHeroReadyCommand)}";
        }

        private async ValueTask<bool> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            var heroState = await AdventureParser.GetHeroStateAsync(browser.CurrentPage);

            if (heroState.Health <= 30)
            {
                return false;
            }

            if (!heroState.IsAtHome)
            {
                return false;
            }

            if (!heroState.IsAdventureReady)
            {
                return false;
            }
            return true;
        }
    }
}