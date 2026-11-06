namespace MainCore.Commands.Features.SetHeroPoint
{
    [Handler]
    public static partial class CheckLevelUpCommand
    {
        public sealed record Command(IChromeBrowser Browser) : ICommand
        {
            public string Name => nameof(CheckLevelUpCommand).Humanize();
        }

        private static async ValueTask<bool> HandleAsync(Command command)
        {
            var browser = command.Browser;
            var canSetHeroPoint = await browser.CurrentPage.Locator("#topBarHero i.levelUp.show").CountAsync() > 0;
            return canSetHeroPoint;
        }
    }
}