namespace MainCore.Features.Navigate;

[Handler]
public sealed partial class SwitchTabCommand(ILogger logger)
{
    public sealed record Command(IChromeBrowser Browser, int TabIndex) : ICommand
    {
        public string Name => $"{nameof(SwitchTabCommand)} [{Description}]";
        private string Description => $"#{TabIndex + 1}";
    }

    private async ValueTask<Result> HandleAsync(Command command)
    {
        var (browser, tabIndex) = command;
        var tabs = browser.CurrentPage.GetTabs();

        var countTabs = await tabs.CountAsync();
        if (countTabs == 0) return Result.Ok();

        if (tabIndex >= countTabs)
        {
            logger.Warning("Found {CountTabs} tabs but need tab #{TabIndex} active", countTabs, tabIndex + 1);
            return Retry.Error;
        }
        var tab = tabs.Nth(tabIndex);
        if (await tab.IsTabActive()) return Result.Ok();

        Result result;
        result = await browser.Click(tab);
        if (result.IsFailed) return result;

        result = await browser.Wait(tab, condition: Parser.TabActiveExpression);
        if (result.IsFailed) return result;
        return Result.Ok();
    }
}