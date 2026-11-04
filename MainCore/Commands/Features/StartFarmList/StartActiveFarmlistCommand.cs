namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class StartActiveFarmlistCommand
    {
        public sealed record Command(IChromeBrowser Browser, List<FarmId> Farms) : ICommand
        {
            public string Name => nameof(StartAllFarmlistCommand).Humanize();
        }

        private static async ValueTask<Result> HandleAsync(Command command)
        {
            var (browser, farmLists) = command;

            Result result;
            foreach (var farmList in farmLists)
            {
                result = await browser.Click(FarmListParser.GetStartButton(browser.CurrentPage, farmList));
                if (result.IsFailed) return result;
            }
            return Result.Ok();
        }
    }
}