namespace MainCore.Features.Navigate;

[Handler]
public static partial class ToDorfCommand
{
    public sealed record Command(IChromeBrowser Browser, int Dorf) : ICommand
    {
        public string Name => $"{nameof(ToDorfCommand)} [{Description}]";
        private string Description => $"{Dorf}";
    }

    private static async ValueTask<Result> HandleAsync(Command command)
    {
        var (browser, dorf) = command;

        var currentDorf = await GetCurrentDorf(browser.CurrentPage);
        if (dorf == 0)
        {
            if (currentDorf == 0) dorf = 2;
            else dorf = currentDorf;
        }

        if (currentDorf != 0 && dorf == currentDorf)
        {
            return Result.Ok();
        }

        Result result;
        result = await browser.Click(browser.CurrentPage.GetDorfButton(dorf));
        if (result.IsFailed) return result;

        result = await browser.Wait(browser.CurrentPage.GetDorf(dorf));
        if (result.IsFailed) return result;

        return Result.Ok();
    }

    private static async Task<int> GetCurrentDorf(IPage page)
    {
        if (await page.GetDorf(1).CountAsync() > 0) return 1;
        if (await page.GetDorf(2).CountAsync() > 0) return 2;
        return 0;
    }
}