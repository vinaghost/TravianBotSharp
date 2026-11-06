namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class StartAllFarmlistCommand
    {
        public sealed record Command(IChromeBrowser Browser) : ICommand
        {
            public string Name => nameof(StartAllFarmlistCommand).Humanize();
        }

        private static async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = command.Browser;
            var result = await browser.Click(FarmListParser.GetStartAllButton(browser.CurrentPage));
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}