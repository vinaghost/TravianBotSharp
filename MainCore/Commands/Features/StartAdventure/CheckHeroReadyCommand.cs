namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public static partial class CheckHeroReadyCommand
    {
        public sealed record Command(IChromeBrowser Browser) : ICommand
        {
            public string Name => nameof(CheckHeroReadyCommand).Humanize();
        }

        private static async ValueTask<bool> HandleAsync(Command command)
        {
            var browser = command.Browser;
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