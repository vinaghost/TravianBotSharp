namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public static partial class OpenAdventurePanelCommand
    {
        public sealed record Command(IChromeBrowser Browser);

        private static async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = command.Browser;
            Result result;

            result = await browser.Click(AdventureParser.GetHeroAdventureButton(browser.CurrentPage));
            if (result.IsFailed) return result;
            result = await browser.WaitPageChanged("hero/adventures");
            if (result.IsFailed) return result;
            result = await browser.Wait(AdventureParser.GetAdventureDurationSection(browser.CurrentPage));
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}